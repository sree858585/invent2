import { createRouter, createWebHistory } from "vue-router";
import DummyPage from "@/components/DummyPage.vue";
import CourseList from "@/components/CourseList.vue";
import MyCourses from "@/components/MyCourses.vue";
import CourseModule from "@/components/CourseModule.vue";
import CourseListPage from "@/components/CourseListPage.vue"; 
import PeerCertification from "@/components/PeerCertification.vue";
import Trending from "@/components/Trending.vue"; 



const routes = [
    {
        path: "/home",
        name: "Home",
        component: DummyPage
    },
    {
        path: "/course-list/:format",
        name: "CourseList",
        component: CourseList,
        props: (route) => ({ format: parseInt(route.params.format) }),
    },
    {
        path: "/course-list-page",
        name: "CourseListPage",
        component: CourseListPage, 
    },
    {
        path: "/peer-certification",
        name: "PeerCertification",
        component: PeerCertification, 
    },
    {
        path: "/trending",
        name: "Trending",
        component: Trending, 
    },
    {
        path: "/my-courses/:status",
        name: "MyCourses",
        component: MyCourses,
        props: route => ({ status: route.params.status })
    },
    {
        path: "/course-module",
        name: "CourseModule",
        component: CourseModule
    },
    {
        path: "/",
        redirect: "/home" // Redirect to home by default
    },
    {
        path: "/:pathMatch(.*)*",
        redirect: "/home" // Fallback route
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
