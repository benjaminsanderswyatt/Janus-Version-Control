﻿using System.Text;

namespace backend.Utils
{
    public class TreeNode
    {
        public string Name { get; set; } // Name of the file or directory
        public string? Hash { get; set; } // Hash of the file (null for directory)
        public List<TreeNode> Children { get; set; } // List of child nodes

        public TreeNode(string name, string hash = null)
        {
            Name = name;
            Hash = hash;
            Children = new List<TreeNode>();
        }

    }

    public class TreeBuilder
    {
        private TreeNode root;
        private string _treeDir;

        public TreeBuilder(int repoId)
        {
            _treeDir = Path.Combine(Environment.GetEnvironmentVariable("TREE_STORAGE_PATH"), repoId.ToString());

            root = new TreeNode("root"); // Root of the tree
        }


        // Build the tree from a dictionary of filepath hash
        public TreeNode BuildTreeFromDiction(Dictionary<string, string> index)
        {
            foreach (var kvp in index)
            {
                string filePath = kvp.Key;
                string fileHash = kvp.Value;

                string[] pathParts = filePath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
                AddToTree(pathParts, fileHash, 0, root);
            }

            return root;
        }


        private void AddToTree(string[] pathParts, string hash, int index, TreeNode current)
        {
            string part = pathParts[index]; // Get the current part of the path

            // Find and or create the child node with the current part
            var child = current.Children.Find(c => c.Name == part);
            if (child == null)
            {
                bool isFile = index == pathParts.Length - 1; // Last part of the path is the file
                child = new TreeNode(part, isFile ? hash : null);
                current.Children.Add(child);
            }

            // Recurse if not at the end of the path
            if (index < pathParts.Length - 1)
            {
                AddToTree(pathParts, hash, index + 1, child);
            }

        }


        public void PrintTree(TreeNode node = null, int level = 0)
        {
            node ??= root; // Start from the root if no node is provided

            string indent = new string(' ', level * 2);
            Console.WriteLine($"{indent}{node.Name} {(node.Hash != null ? $"({node.Hash})" : "")}");

            foreach (var child in node.Children)
            {
                PrintTree(child, level + 1);
            }
        }


        public string SaveTree()
        {
            return SaveTreeRecursively(root);
        }

        private string SaveTreeRecursively(TreeNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var lines = new List<string>();

            foreach (var child in node.Children.OrderBy(c => c.Name))
            {
                if (child.Hash != null)
                {
                    // File entry
                    lines.Add($"blob|{child.Name}|{child.Hash}");
                }
                else
                {
                    // Directory entry
                    string childHash = SaveTreeRecursively(child);
                    lines.Add($"tree|{child.Name}|{childHash}");
                }
            }

            // Create the content for this directory
            string content = string.Join("\n", lines);
            string hash = HashHelper.ComputeHash(content);

            // Save the directory file (dont overide if already exists)
            string filePath = Path.Combine(_treeDir, hash);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
            }

            return hash;
        }


        public TreeNode RecreateTree(string treeHash)
        {
            if (string.IsNullOrEmpty(treeHash)) // Initial commit
            {
                return new TreeNode("root");
            }

            TreeNode node = RebuildTreeRecursive(treeHash);

            root = node;
            return node;
        }


        private TreeNode RebuildTreeRecursive(string treeHash)
        {
            try
            {
                // Load the tree content from storage
                string treePath = Path.Combine(_treeDir, treeHash);
                string[] treeContent = File.ReadAllLines(treePath, Encoding.UTF8);


                // Create a new TreeNode for the current tree
                var treeNode = new TreeNode("root");

                foreach (var line in treeContent)
                {
                    var parts = line.Split('|');

                    if (parts.Length >= 3)
                    {
                        string type = parts[0]; // blob or tree
                        string name = parts[1]; // Name of the file or directory
                        string hash = parts[2]; // Hash of the file or directory

                        if (type == "tree")
                        {
                            // Recursively rebuild the child tree
                            var childTree = RebuildTreeRecursive(hash);
                            if (childTree != null)
                            {
                                childTree.Name = name;
                                treeNode.Children.Add(childTree);
                            }
                        }
                        else if (type == "blob")
                        {
                            // Add file node
                            var fileNode = new TreeNode(name, hash);
                            treeNode.Children.Add(fileNode);
                        }
                    }
                }


                return treeNode;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to recreate tree", ex);
                //return null;
            }
        }

    }


    // Represents the results of comparing two trees
    public class TreeComparisonResult
    {
        public List<string> AddedOrUntracked { get; set; } = new(); // GetAddedModifiedDeleted -> Added, GetNotStagedUntracked -> Untracked
        public List<string> ModifiedOrNotStaged { get; set; } = new(); // GetAddedModifiedDeleted -> Modified, GetNotStagedUntracked -> Not Staged
        public List<string> Deleted { get; set; } = new();
    }

    public static class Tree
    {
        public static TreeComparisonResult CompareTrees(TreeNode tree1, TreeNode tree2)
        {
            var result = new TreeComparisonResult();

            if (tree1 == null && tree2 == null)
            {
                // Both trees are empty -> no changes
                return result;
            }

            if (tree1 == null)
            {
                // All files in tree2 are added
                CollectPaths(tree2, "", result.AddedOrUntracked);
                return result;
            }

            if (tree2 == null)
            {
                // All files in tree1 are deleted
                CollectPaths(tree1, "", result.Deleted);
                return result;
            }

            // Compare
            CompareNodes(tree1, tree2, "", result);

            return result;
        }


        private static void CollectPaths(TreeNode node, string currentPath, List<string> result)
        {
            if (node == null) return;

            foreach (var child in node.Children)
            {
                string path = Path.Combine(currentPath, child.Name);

                if (child.Hash != null)
                {
                    // Add file path to the result
                    result.Add(path);
                }
                else
                {
                    // Recurse into sub directories
                    CollectPaths(child, path, result);
                }
            }
        }


        private static void CompareNodes(TreeNode node1, TreeNode node2, string currentPath, TreeComparisonResult result)
        {
            // Collect children as dictionaries for easier lookup
            var node1Files = node1?.Children?.Where(c => c.Hash != null).ToDictionary(c => c.Name, c => c) ?? new Dictionary<string, TreeNode>();
            var node2Files = node2?.Children?.Where(c => c.Hash != null).ToDictionary(c => c.Name, c => c) ?? new Dictionary<string, TreeNode>();

            // Combine keys of all files in both trees
            var allKeys = new HashSet<string>(node1Files.Keys.Concat(node2Files.Keys));

            foreach (var key in allKeys)
            {
                string path = Path.Combine(currentPath, key);

                if (!node1Files.ContainsKey(key)) // Added
                {
                    // File is only in tree2
                    result.AddedOrUntracked.Add(path);
                }
                else if (!node2Files.ContainsKey(key) || node2Files[key].Hash == "Deleted") // Deleted
                {
                    // File is only in tree1 (not in tree2)
                    result.Deleted.Add(path);
                }
                else
                {
                    // File exists in both
                    var file1 = node1Files[key];
                    var file2 = node2Files[key];

                    if (file1.Hash != file2.Hash) // Modified
                    {
                        result.ModifiedOrNotStaged.Add(path);
                    }
                }
            }

            // Recurse into subdirectories ignore empty folders
            var node1Dirs = node1?.Children?.Where(c => c.Hash == null) ?? Enumerable.Empty<TreeNode>();
            var node2Dirs = node2?.Children?.Where(c => c.Hash == null) ?? Enumerable.Empty<TreeNode>();

            var allDirs = new HashSet<string>(node1Dirs.Select(d => d.Name).Concat(node2Dirs.Select(d => d.Name)));

            foreach (var dirName in allDirs)
            {
                var childNode1 = node1Dirs.FirstOrDefault(d => d.Name == dirName);
                var childNode2 = node2Dirs.FirstOrDefault(d => d.Name == dirName);

                CompareNodes(childNode1, childNode2, Path.Combine(currentPath, dirName), result);
            }
        }







    }


}
