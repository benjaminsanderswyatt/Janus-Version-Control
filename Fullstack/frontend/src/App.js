import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router";


import { AuthProvider } from './providers/AuthContext';
import useTokenRefresh from './trying/refreshToken';


import Layout from './pages/Layout';
import NoPage from './pages/NoPage';

import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';

import Repositories from './pages/Repositories';
import Account from './pages/Account';

import TermsOfUse from './pages/legal/TermsOfUse'
import PrivacyPolicy from './pages/legal/PrivacyPolicy';

import { ThemeProvider, useTheme } from './providers/ThemeContext';
import './styles/App.css';



// ProtectedRoute you can only access if you have valid Json Web Token
const ProtectedRoute = () => {
  const token = localStorage.getItem('token'); // Check for token in localStorage

  // If token exists, render the requested component
  return token ? <Outlet /> : <Navigate to="/" replace />;
};

const App = () => {
  const { theme } = useTheme();

  useTokenRefresh(); // Automatically refresh the token


  // Set the theme from ThemeContext useTheme
  useEffect(() => {
    document.body.setAttribute('data-theme', theme);
  }, [theme]);


  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>

            {/*Default route*/}
            <Route index element={<Home />} />


            {/*User Pages*/}
            <Route path="login">
              <Route index element={<Login />}/>
            </Route>

            <Route path="register">
              <Route index element={<Register />}/>
            </Route>



            {/*Protected Routes*/}
            <Route path="repositories" element={<ProtectedRoute />}>
              <Route index element={<Repositories />}/>
            </Route>

            <Route path="account" element={<ProtectedRoute />}>
              <Route index element={<Account />}/>
            </Route>





            {/* Legal Pages */}
            <Route path="legal/termsofuse" element={<TermsOfUse />} />
            <Route path="legal/privacypolicy" element={<PrivacyPolicy />} />

            {/*Catch all invalid routes (404)*/}
            <Route path="*" element={<NoPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

// Wrap app in the theme and export that as the new app
const AppWithTheme = () => (
  <ThemeProvider>
    <App />
  </ThemeProvider>
);

export default AppWithTheme;
