import React from 'react';
import { useTheme } from '../providers/ThemeContext';

// Componant to toggle between light and dark theme
const ThemeToggle = () => {
  const { theme, toggleTheme } = useTheme();

  
  return (
    <button onClick={toggleTheme}>
      Switch to {theme === 'light' ? 'Dark' : 'Light'} Mode
    </button>
  );
};

export default ThemeToggle;
