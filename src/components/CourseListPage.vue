<template>
    <div class="courses-container">
        <h1>Register for a Course</h1>
        <div v-if="loading" class="loading">Loading courses...</div>
        <table v-else-if="courses.length > 0" class="styled-table">
            <thead>
                <tr>
                    <th>Course Title</th>
                    <th>Date</th>
                    <th>Region</th>
                    <th>Format</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="course in courses" :key="course.courseSysId">
                    <td>{{ course.information || "N/A" }}</td>
                    <td>{{ formatDate(course.courseDate) }}</td>
                    <td>{{ course.region || "N/A" }}</td>
                    <td>{{ formatMap[course.format] || "N/A" }}</td>
                    <td>
                        <button class="btn-primary" @click="register(course)">Select</button>
                    </td>
                </tr>
            </tbody>
        </table>
        <p v-else class="no-courses">No courses available at the moment.</p>
    </div>
</template>

<script>import { CourseService } from "@/services/CourseService";

    export default {
        name: "CourseListPage",
        data() {
            return {
                courses: [],
                loading: true,
                formatMap: {
                    1: "In Person",
                    2: "Online",
                    3: "Archived Webinars",
                    4: "Live Webinars",
                    5: "Hybrid",
                    6: "New",
                },
            };
        },
        created() {
            this.loadCourses();
        },
        methods: {
            async loadCourses() {
                try {
                    const response = await CourseService.getCourses();
                    this.courses = response.data?.$values ?? response.data;
                } catch (error) {
                    console.error("Error fetching courses:", error);
                } finally {
                    this.loading = false;
                }
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
    .page-container {
        display: flex;
        height: 100vh;
        overflow: hidden;
    }

        /* SideNav Styling */
        .page-container .sidenav-container {
            flex: 0 0 250px;
        }

    /* Content Container Styling */
    .content-container {
        flex: 1;
        padding: 20px;
        background-color: #f9f9f9;
        overflow-y: auto;
    }

    /* Heading */
    h1 {
        text-align: center;
        font-size: 2rem;
        color: #3f51b5;
        margin-bottom: 20px;
    }

    /* Loading State */
    .loading {
        font-size: 1.5rem;
        color: #666;
        text-align: center;
        margin-top: 20px;
    }

    /* Table Styling */
    .styled-table {
        width: 100%;
        border-collapse: collapse;
        border-radius: 8px;
        box-shadow: 0 6px 10px rgba(0, 0, 0, 0.1);
        overflow: hidden;
    }

        .styled-table th,
        .styled-table td {
            padding: 15px;
            text-align: center;
            border-bottom: 1px solid #ddd;
            color: #333;
        }

        .styled-table th {
            background-color: #3f51b5;
            color: #fff;
            text-transform: uppercase;
        }

        .styled-table tr:nth-child(even) {
            background-color: #f2f2f2;
        }

        .styled-table tr:hover {
            background-color: #e0f7fa;
            transition: background-color 0.3s ease;
        }

    /* Button Styling */
    .btn-primary {
        background-color: #3f51b5;
        color: #fff;
        border: none;
        padding: 8px 12px;
        border-radius: 6px;
        cursor: pointer;
        transition: background-color 0.3s ease;
    }

        .btn-primary:hover {
            background-color: #2c3e50;
        }

    /* No Courses */
    .no-courses {
        font-size: 1.2rem;
        color: #666;
        text-align: center;
        margin-top: 20px;
    }
</style>
