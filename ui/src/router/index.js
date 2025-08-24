import AppLayout from '@/layout/AppLayout.vue';
import { createRouter, createWebHistory } from 'vue-router';

import {userStoreMe} from "@/store/userStore";


const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            component: AppLayout,
            meta: { requiresAuth: true },
            children: [
                {
                    path: '/',
                    name: 'home',
                    component: () => import('@/views/Index.vue')
                },
                {
                    path: '/pages/sqleditor',
                    name: 'sqleditor',
                    component: () => import('@/views/pages/SqlEditor.vue')
                },
                {
                    path: '/pages/dataset',
                    name: 'dataset',
                    component: () => import('@/views/pages/Dataset.vue')
                },
                {
                    path: '/pages/databases',
                    name: 'databases',
                    component: () => import('@/views/pages/Databases.vue')
                },
                {
                    path: '/pages/cseditor',
                    name: 'cseditor',
                    component: () => import('@/views/pages/CsEditor.vue')
                },
                {
                    path: '/pages/dashboard',
                    name: 'dashboard',
                    component: () => import('@/views/pages/Dashboard.vue')
                },
                {
                    path: '/pages/empty',
                    name: 'empty',
                    component: () => import('@/views/pages/Empty.vue')
                },
                {
                    path: '/pages/crud',
                    name: 'crud',
                    component: () => import('@/views/pages/Crud.vue')
                },
                {
                    path: '/documentation',
                    name: 'documentation',
                    component: () => import('@/views/pages/Documentation.vue')
                }
            ]
        },
        {
            path: '/pages/notfound',
            name: 'notfound',
            component: () => import('@/views/pages/NotFound.vue')
        },

        {
            path: '/auth/login',
            name: 'login',
            component: () => import('@/views/pages/auth/Login.vue')
        },
        {
            path: '/auth/access',
            name: 'accessDenied',
            component: () => import('@/views/pages/auth/Access.vue')
        },
        {
            path: '/auth/error',
            name: 'error',
            component: () => import('@/views/pages/auth/Error.vue')
        }
    ]
});


router.beforeEach((to, from, next) => {
    // Comprueba si la ruta a la que se intenta acceder requiere autenticación
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

    // Aquí asumimos que guardas un token en localStorage al iniciar sesión
    const isAuthenticated = localStorage.getItem('user-token');

    const userStore = userStoreMe();
    //console.log(userStore);
    if (requiresAuth && !userStore.auth) {
        // Si la ruta requiere autenticación y el usuario no está autenticado, redirige a la página de login
        next({ name: 'login' });
    } else {
        // De lo contrario, permite la navegación
        next();
    }
});

export default router;
