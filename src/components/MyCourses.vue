<template>
    <div class="my-courses-container">
        <h1>My Courses</h1>
        <div v-if="loading" class="loading">Loading courses...</div>
        <div v-else>
            <div class="tabs">
                <button v-for="tab in tabs"
                        :key="tab.label"
                        :class="{ active: activeTab === tab.key }"
                        @click="activeTab = tab.key">
                    {{ tab.label }}
                </button>
            </div>

            <div class="course-section" v-if="activeTab === 'inProgress'">
                <h2>In Progress</h2>
                <table class="styled-table">
                    <thead>
                        <tr>
                            <th>Course Title</th>
                            <th>Training Center</th>
                            <th>Date</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="course in inProgressCourses" :key="course.title">
                            <td>{{ course.title }}</td>
                            <td>{{ course.trainingCenter }}</td>
                            <td>{{ formatDate(course.date) }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="course-section" v-if="activeTab === 'registered'">
                <h2>Registered</h2>
                <table class="styled-table">
                    <thead>
                        <tr>
                            <th>Course Title</th>
                            <th>Training Center</th>
                            <th>Date</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="course in registeredCourses" :key="course.title">
                            <td>{{ course.title }}</td>
                            <td>{{ course.trainingCenter }}</td>
                            <td>{{ formatDate(course.date) }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="course-section" v-if="activeTab === 'completed'">
                <h2>Completed</h2>
                <table class="styled-table">
                    <thead>
                        <tr>
                            <th>Course Title</th>
                            <th>Training Center</th>
                            <th>Date</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="course in completedCourses" :key="course.title">
                            <td>{{ course.title }}</td>
                            <td>{{ course.trainingCenter }}</td>
                            <td>{{ formatDate(course.date) }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<script>export default {
        name: "MyCoursesPage",
        data() {
            return {
                loading: false,
                activeTab: "inProgress",
                tabs: [
                    { label: "In Progress", key: "inProgress" },
                    { label: "Registered", key: "registered" },
                    { label: "Completed", key: "completed" },
                ],
                inProgressCourses: [
                    { title: "HIV Basics", trainingCenter: "AIDS Institute", date: "2024-01-10" },
                    { title: "HIV Advanced", trainingCenter: "AIDS Institute", date: "2024-01-15" },
                ],
                registeredCourses: [
                    { title: "HIV Treatment", trainingCenter: "AIDS Institute", date: "2024-02-05" },
                ],
                completedCourses: [
                    { title: "HIV Awareness", trainingCenter: "AIDS Institute", date: "2023-12-01" },
                ],
            };
        },
        methods: {
            formatDate(date) {
                return new Date(date).toLocaleDateString();
            },
        },
    };</script>

<style scoped>
    .my-courses-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        height: 100vh;
        padding: 20px;
        box-sizing: border-box;
        background-color: #f4f6f8;
    }

    h1 {
        text-align: center;
        color: #3f51b5;
        margin-bottom: 20px;
    }

    .loading {
        display: flex;
        justify-content: center;
        align-items: center;
        font-size: 1.5rem;
        color: #666;
        flex-grow: 1;
    }

    .tabs {
        display: flex;
        justify-content: center;
        gap: 10px;
        margin-bottom: 20px;
        width: 100%;
    }

        .tabs button {
            flex: 1;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            background-color: #e0e0e0;
            cursor: pointer;
            transition: background-color 0.3s ease;
            font-weight: bold;
            text-align: center;
        }

            .tabs button.active {
                background-color: #3f51b5;
                color: white;
            }

    .course-section {
        flex-grow: 1;
        width: 100%;
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    h2 {
        color: #3f51b5;
        margin-bottom: 20px;
        text-align: left;
        width: 90%;
    }

    .styled-table {
        width: 95%;
        border-collapse: collapse;
        background-color: white;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 8px 12px rgba(0, 0, 0, 0.1);
    }

        .styled-table th,
        .styled-table td {
            padding: 15px;
            text-align: center;
            border-bottom: 1px solid #ddd;
        }

        .styled-table th {
            background-color: #3f51b5;
            color: white;
            text-transform: uppercase;
        }

        .styled-table tr:nth-child(even) {
            background-color: #f2f2f2;
        }

        .styled-table tr:hover {
            background-color: #e3f2fd;
        }
</style>
