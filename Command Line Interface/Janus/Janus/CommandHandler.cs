﻿using Janus.Plugins;

namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new InitCommand(),
                new AddCommand(),
                new CommitCommand(),
                new CreateBranchCommand(),
                new SwitchBranchCommand(),
                new LogCommand()

                // Add new built in commands here
            };
        }


        public class InitCommand : ICommand
        {
            public string Name => "init";
            public string Description => "Initializes the repository.";
            public void Execute(string[] args)
            {
                // Initialise .janus folder
                if (!Directory.Exists(Paths.janusDir))
                {
                    Directory.CreateDirectory(Paths.janusDir);
                }
                else
                {
                    Console.WriteLine("Repository already initialized");
                    return;
                }

                File.SetAttributes(Paths.janusDir, File.GetAttributes(Paths.janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden

                // .janus/object folder
                if (!Directory.Exists(Paths.objectDir))
                    Directory.CreateDirectory(Paths.objectDir);

                // .janus/refs
                if (!Directory.Exists(Paths.refsDir))
                    Directory.CreateDirectory(Paths.refsDir);

                // .janus/refs/heads
                if (!Directory.Exists(Paths.headsDir))
                    Directory.CreateDirectory(Paths.headsDir);

                // .janus/plugins folder
                if (!Directory.Exists(Paths.pluginsDir))
                    Directory.CreateDirectory(Paths.pluginsDir);


                // Create index file
                File.Create(Paths.index).Close();

                // Create empty main branch in refs/heads/
                File.WriteAllText(Path.Combine(Paths.headsDir, "main"), string.Empty);

                // Create HEAD file pointing at main branch
                File.WriteAllText(Paths.head, "ref: refs/heads/main");

                Console.WriteLine("Initialized janus repository");
            }
        }

        public class AddCommand : ICommand
        {
            public string Name => "add";
            public string Description => "add help";
            public void Execute(string[] args)
            {
                var metadata = CommandHelper.LoadMetadata();
                var filesToAdd = new List<string>();

                if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    filesToAdd = Directory.EnumerateFiles(".", "*", SearchOption.AllDirectories)
                                  .Select(filePath => Path.GetRelativePath(".", filePath))
                                  .Where(relativePath => relativePath.StartsWith(".janus"))
                                  .ToList();
                    
                    // Stages all changes
                    foreach (string filePath in Directory.GetFiles(".", "*", SearchOption.AllDirectories))
                    {
                        string relativePath = Path.GetRelativePath(".", filePath);

                        // Dont stage the .janus files
                        if (relativePath.StartsWith(".janus"))
                        {
                            continue;
                        }


                        filesToAdd.Add(relativePath);
                    }

                    // Handles deleted files
                    foreach (var trackedFile in metadata.Files.Keys.ToList())
                    {
                        if (!File.Exists(trackedFile))
                        {
                            metadata.Files[trackedFile].LastCommitHash = null;
                            Console.WriteLine($"Staged {trackedFile} for deletion.");
                        }
                    }
                } 
                else
                {
                    filesToAdd.AddRange(args);
                }

                foreach (string fileName in filesToAdd)
                {

                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"File '{fileName}' not found.");
                        continue;
                    }

                    string currentContent = File.ReadAllText(filePath);

                    var fileMetadata = metadata.Files.ContainsKey(fileName) ? metadata.Files[fileName] : new FileMetadata();

                    string currentHash = CommandHelper.GetHash(currentContent);
                    string? previousBlobHash = fileMetadata.LastCommitHash;

                    if (previousBlobHash != null)
                    {
                        string previousContent = CommandHelper.LoadBlob(previousBlobHash);
                        if (currentHash != previousBlobHash)
                        {
                            string delta = CommandHelper.GetDelta(previousContent, currentContent);
                            string deltaHash = CommandHelper.SaveDeltaNode(new DeltaNode
                            {
                                Content = delta,
                                NextDeltaHash = fileMetadata.DeltaHeadHash
                            });

                            // Update file metadata to point to new delta
                            fileMetadata.DeltaHeadHash = deltaHash;
                            Console.WriteLine($"Added delta for {fileName} with hash {deltaHash}");

                        }
                        else
                        {
                            Console.WriteLine($"{fileName} hasnt changed");
                        }
                    }
                    else
                    {
                        // First time file has been added (save the whole blob)
                        string blobHash = CommandHelper.SaveBlob(filePath);
                        fileMetadata.LastCommitHash = blobHash;
                        fileMetadata.DeltaHeadHash = null;
                        Console.WriteLine($"Added: {fileName}, blob: {blobHash}");
                    }

                    metadata.Files[fileName] = fileMetadata;
                }

                CommandHelper.SaveMetadata(metadata);

            }
        }

        public class CommitCommand : ICommand
        {
            public string Name => "commit";
            public string Description => "commit";
            public void Execute(string[] args)
            {
                string message = args[0];

                // Get files from staging area
                var files = new Dictionary<string, string>();

                foreach (var line in File.ReadLines(Paths.index))
                {
                    var parts = line.Split(' ');
                    files[parts[0]] = parts[1];
                }

                string treeHash = CommandHelper.SaveTree(files);
                string commitHash = CommandHelper.SaveCommit(treeHash, message);

                var metadata = CommandHelper.LoadMetadata();
                foreach (var file in files.Keys)
                {
                    if (metadata.Files.ContainsKey(file))
                    {
                        metadata.Files[file].LastCommitHash = commitHash;
                    }
                }
                CommandHelper.SaveMetadata(metadata);

                // Clear staging area (index)
                File.WriteAllText(Paths.index, string.Empty);

                // Write current commit hash to refs/heads/[current branch]
                string currentBranch = File.ReadAllText(Paths.head).Replace("ref: ", "").Trim();
                string branchPath = Path.Combine(Paths.janusDir, currentBranch);
                File.WriteAllText(branchPath, commitHash);

                Console.WriteLine($"Committed to {currentBranch} with hash {commitHash}");
            }
        }





        public class CreateBranchCommand : ICommand
        {
            public string Name => "create branch";
            public string Description => "create branch help";
            public void Execute(string[] args)
            {
                string branchName = args[0];
                string branchPath = Path.Combine(Paths.headsDir, branchName);

                if (File.Exists(branchPath))
                {
                    Console.WriteLine($"Branch '{branchName}' already exists.");
                    return;
                }

                // Get the latest commit of the curent branch
                string headPath = Path.Combine(Paths.janusDir, "HEAD");
                string currentBranch = File.ReadAllText(headPath).Replace("ref: ", "").Trim();
                string currentBranchPath = Path.Combine(Paths.janusDir, currentBranch);

                // Write the latest commit hash into the new branch file
                string latestCommitHash = File.ReadAllText(currentBranchPath).Trim();
                File.WriteAllText(branchPath, latestCommitHash);


                Console.WriteLine($"Created new branch {branchName}");
            }
        }

        public class SwitchBranchCommand : ICommand
        {
            public string Name => "switch branch";
            public string Description => "switch branch help";
            public void Execute(string[] args)
            {
                string branchName = args[0];
                string branchPath = Path.Combine(Paths.headsDir, branchName);

                if (File.Exists(branchPath))
                {
                    // Update HEAD to point to the switched to branch
                    File.WriteAllText(Path.Combine(Paths.janusDir, "HEAD"), $"ref: refs/heads/{branchName}");
                    Console.WriteLine($"Switched to branch {branchName}.");
                }
                else
                {
                    Console.WriteLine($"Branch {branchName} does not exist.");
                }
            }
        }

        public class LogCommand : ICommand
        {
            public string Name => "log";
            public string Description => "log help";
            public void Execute(string[] args)
            {
                foreach (var commitFile in Directory.GetFiles(Paths.objectDir))
                {
                    string content = File.ReadAllText(commitFile);
                    if (content.StartsWith("Tree:"))
                    {
                        Console.WriteLine(content);
                    }
                }
            }
        }

    }
}
