<template>
    <div class="course-container">
        <h2 class="heading">Courses for {{ formatName(format) }}</h2>
        <div v-if="loading" class="loading">Loading courses...</div>
        <div v-else-if="courses.length > 0" class="course-grid">
            <div v-for="course in courses" :key="course.courseSysId" class="card">
                <div class="card-body">
                    <h5 class="card-title">{{ course.information || "Untitled Course" }}</h5>
                    <p><strong>Date:</strong> {{ formatDate(course.courseDate) }}</p>
                    <p><strong>Time:</strong> {{ course.courseTime || "N/A" }}</p>
                    <p><strong>Location:</strong> {{ course.city || "Unknown City" }}, {{ course.trainingLocation || "Unknown Location" }}</p>
                    <p><strong>Subject:</strong> {{ course.subjectTitle || "No Subject" }}</p>
                    <button class="btn-primary" @click="register(course)">Register</button>
                </div>
            </div>
        </div>
        <p v-else class="no-data">No courses available for this format.</p>
    </div>
</template>

<script>import { CourseService } from "@/services/CourseService";

    export default {
        name: "CourseList",
        props: ["format"],
        data() {
            return {
                courses: [],
                loading: true,
            };
        },
        created() {
            this.getCourses();
        },
        watch: {
            format() {
                this.getCourses();
            },
        },
        methods: {
            async getCourses() {
                this.loading = true;
                try {
                    const response = await CourseService.getCoursesByFormat(this.format);
                    this.courses = response.data?.$values ?? response.data;
                } catch (error) {
                    console.error("Error fetching courses:", error);
                } finally {
                    this.loading = false;
                }
            },
            formatName(format) {
                const categories = {
                    1: "In Person",
                    2: "Online",
                    3: "Archived Webinars",
                    4: "Live Webinars",
                    5: "Hybrid",
                    6: "New",
                };
                return categories[format] || "Unknown";
            },
            formatDate(date) {
                return new Date(date).toLocaleDateString();
            },
            register(course) {
                console.log(`Register clicked for course: ${course.courseSysId}`);
            },
        },
    };</script>

<style scoped>
    /* General Container */
    .course-container {
        margin: 0; /* Remove margin */
        padding: 20px;
        width: 100%; /* Full width */
        min-height: 100vh; /* Full height */
        background-color: #f4f6f8;
        box-sizing: border-box; /* Ensures padding is included in the width */
    }

    .heading {
        text-align: center;
        font-size: 2.5rem;
        margin-bottom: 20px;
        color: #3f51b5;
    }

    /* Loading State */
    .loading {
        text-align: center;
        font-size: 1.5rem;
        color: #666;
        margin-top: 20px;
    }

    /* Course Grid */
    .course-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); /* Flexible grid layout */
        gap: 20px;
        padding: 10px;
        width: 100%;
    }

    /* Card Styling */
    .card {
        background: linear-gradient(to bottom right, #ffffff, #f9f9f9);
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 6px 10px rgba(0, 0, 0, 0.1);
        transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

        .card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
        }

    .card-body {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .card-title {
        font-size: 1.8rem;
        font-weight: bold;
        color: #333;
        margin-bottom: 10px;
    }

    /* Button Styling */
    .btn-primary {
        background-color: #3f51b5;
        color: #fff;
        padding: 10px 16px;
        border: none;
        border-radius: 8px;
        font-weight: bold;
        text-transform: uppercase;
        transition: background-color 0.3s ease, transform 0.3s ease;
        cursor: pointer;
        align-self: flex-start;
    }

        .btn-primary:hover {
            background-color: #2c3e50;
            transform: scale(1.05);
        }

    /* No Data Message */
    .no-data {
        text-align: center;
        font-size: 1.5rem;
        color: #999;
        margin-top: 20px;
    }
</style>
