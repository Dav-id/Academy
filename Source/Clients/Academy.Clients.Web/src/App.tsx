import React, { StrictMode, useState } from 'react'
//import { AuthProvider, useAuth } from './lib/auth/AuthContext';

import { RouterProvider } from "react-router";
import router from './routes';

export default function App() {
    return (
        <RouterProvider router={router} />
    );
}
