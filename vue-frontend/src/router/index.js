import { createRouter, createWebHistory } from "vue-router";
import DummyPage from "@/components/DummyPage.vue";
import CourseList from "@/components/CourseList.vue";
import MyCourses from "@/components/MyCourses.vue";
import CourseModule from "@/components/CourseModule.vue";
import CourseListPage from "@/components/CourseListPage.vue";
import PeerCertification from "@/components/PeerCertification.vue";
import Trending from "@/components/Trending.vue";
import ProfileComponent from "@/components/ProfileComponent.vue";
import CourseManagement from "@/components/CourseManagement.vue";
import TrainingTitle from "@/components/TrainingTitle.vue";
import TrainingCenter from "@/components/TrainingCenter.vue";
import InstructorManagement from "@/components/InstructorManagement.vue";
import CourseListManager from "@/components/CourseListManager.vue";
import MarkAttendance from "@/components/MarkAttendance.vue";
import ViewAttendance from "@/components/ViewAttendance.vue";
import RoleManagement from "@/components/RoleManagement.vue";
import TrainingCalendar from "@/components/TrainingCalendar.vue";
import PeerCertificationApply from "@/components/PeerCertificationApply.vue";
import HomeBannersAdmin from "@/components/HomeBannersAdmin.vue";
import ManagePeer from "@/components/ManagePeer.vue";
import ManageEduCredits from "@/components/ManageEduCredits.vue";
import ManagePeerDetail from "@/components/ManagePeerDetail.vue";
import MyCertificates from "@/components/MyCertificates.vue";

import CustomCalendarEventsAdmin from "@/components/CustomCalendarEventsAdmin.vue";

const requireAdminOrManager = (to, from, next) => {
    const isAuthenticated = !!localStorage.getItem("jwtToken");
    const role = localStorage.getItem("userRole"); // must exist
    const allowed = role === "Admin" || role === "Manager";

    if (!isAuthenticated) return next("/home");
    if (!allowed) return next("/home");

    next();
};

const routes = [
    {
        path: "/home",
        name: "Home",
        component: DummyPage,
    },
    {
        path: "/profile/view/:id",
        name: "UserProfile",
        component: ProfileComponent,
        props: true,
        beforeEnter: (to, from, next) => {
            const isAuthenticated = !!localStorage.getItem("jwtToken");
            if (!isAuthenticated) next("/home");
            else next();
        },
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
        path: "/my-certificates",
        name: "MyCertificates",
        component: MyCertificates,
        beforeEnter: (to, from, next) => {
            const isAuthenticated = !!localStorage.getItem("jwtToken");
            if (!isAuthenticated) next("/home");
            else next();
        },
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
        props: (route) => ({ status: route.params.status }),
    },
    {
        path: "/peer-certification/apply",
        name: "PeerCertificationApply",
        component: PeerCertificationApply,
    },
    {
        path: "/course-module",
        name: "CourseModule",
        component: CourseModule,
    },
    {
        path: "/course-management",
        name: "CourseManagement",
        component: CourseManagement,
    },
    {
        path: "/system/training-title",
        name: "TrainingTitle",
        component: TrainingTitle,
    },
    {
        path: "/system/training-center",
        name: "TrainingCenter",
        component: TrainingCenter,
    },
    {
        path: "/system/instructor-management",
        name: "InstructorManagement",
        component: InstructorManagement,
    },
    {
        path: "/system/course-list",
        name: "CourseListManager",
        component: CourseListManager,
    },

    // ✅ NEW ROUTE
    {
        path: "/system/custom-calendar-events",
        name: "CustomCalendarEventsAdmin",
        component: CustomCalendarEventsAdmin,
        beforeEnter: requireAdminOrManager,
    },
    {
        path: "/system/home-banners",
        name: "HomeBannersAdmin",
        component: HomeBannersAdmin,
        beforeEnter: requireAdminOrManager,
    },

    {
        path: "/attendance/mark",
        name: "MarkAttendance",
        component: MarkAttendance,
    },
    {
        path: "/attendance/view",
        name: "ViewAttendance",
        component: ViewAttendance,
    },
    {
        path: "/role-management",
        name: "RoleManagement",
        component: RoleManagement,
    },
    {
        path: "/training-calendar",
        name: "TrainingCalendar",
        component: TrainingCalendar,
    },
    {
        path: "/peer-management/manage-peer",
        name: "ManagePeer",
        component: ManagePeer,
        beforeEnter: requireAdminOrManager,
    },
    {
        path: "/peer-management/manage-edu-credits",
        name: "ManageEduCredits",
        component: ManageEduCredits,
        beforeEnter: requireAdminOrManager,
    },
    {
        path: "/peer-management/manage-peer/:userId",
        name: "ManagePeerDetail",
        component: ManagePeerDetail,
        props: true,
        beforeEnter: requireAdminOrManager,
    },

    {
        path: "/peer-certification/continuing-education",
        name: "PeerContinuingEducation",
        component: () => import("@/components/PeerContinuingEducation.vue")
    },
    {
        path: "/",
        redirect: "/home",
    },
    {
        path: "/:pathMatch(.*)*",
        redirect: "/home",
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;