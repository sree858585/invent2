<template>
    <div class="course-management-container">
        <h2>Course Management</h2>
        <p>Manage all courses from this page.</p>

        <!-- Schedule a New Course Button -->
        <button class="schedule-btn" @click="isModalOpen = true">➕ Schedule a New Course</button>

        <!-- Import and Use the Modal -->
        <ScheduleCourseModal :isOpen="isModalOpen" @close="isModalOpen = false" @submit="handleNewCourse" />
    </div>
</template>

<script>import ScheduleCourseModal from "@/components/ScheduleCourseModal.vue";

export default {
    name: "CourseManagement",
    components: {
        ScheduleCourseModal
    },
    data() {
        return {
            isModalOpen: false
        };
    },
    methods: {
        handleNewCourse(courseData) {
            console.log("New Course Scheduled:", courseData);
        }
    }
};</script>
