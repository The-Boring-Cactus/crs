import { createRouter, createWebHistory } from 'vue-router';
import {userStoreMe} from "@/store/userStore";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            name: 'home',
            component: () => import('@/views/Index.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/sqleditor',
            name: 'sqleditor',
            component: () => import('@/views/pages/SqlEditor.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/dataset',
            name: 'dataset',
            component: () => import('@/views/pages/Dataset.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/myexcel',
            name: 'myexcel',
            component: () => import('@/views/pages/MyExcel.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/databases',
            name: 'databases',
            component: () => import('@/views/pages/Databases.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/cseditor',
            name: 'cseditor',
            component: () => import('@/views/pages/CsEditor.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/dashboard',
            name: 'dashboard',
            component: () => import('@/views/pages/Dashboard.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/empty',
            name: 'empty',
            component: () => import('@/views/pages/Empty.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/crud',
            name: 'crud',
            component: () => import('@/views/pages/Crud.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/documentation',
            name: 'documentation',
            component: () => import('@/views/pages/Documentation.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/pages/charts',
            name: 'charts',
            component: () => import('@/views/pages/ChartDemo.vue'),
            meta: { requiresAuth: true }
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
    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
    const userStore = userStoreMe();

    if (requiresAuth && !userStore.auth) {
        // User not authenticated, but since we handle login in App.vue now, just proceed
        // The App.vue will show login screen if not authenticated
        next();
    } else {
        next();
    }
});

export default router;
