<template>
    <div class="sidenav-container">
        <!-- Hamburger Button -->
        <div class="hamburger-button" @click="toggleSidenav">
            <span>&#9776;</span>
        </div>

        <!-- Side Navigation -->
        <nav :class="{ expanded: isExpanded, collapsed: !isExpanded }">
            <!-- Profile Section -->
            <div class="profile-section" v-show="isExpanded">
                <div class="profile-header">
                    <span class="icon">👤</span>
                    <span>My Profile</span>
                </div>
                <ul>
                    <li><router-link to="/profile/view">View Profile</router-link></li>
                    <li><router-link to="/profile/edit">Edit Profile</router-link></li>
                </ul>
            </div>

            <!-- Navigation Items -->
            <ul class="nav-items">
                <li>
                    <router-link to="/home">
                        <span class="icon">🏠</span>
                        <span v-show="isExpanded">Home</span>
                    </router-link>
                </li>
                <li>
                    <router-link to="/course-list-page">
                        <span class="icon">🗂️</span>
                        <span v-show="isExpanded">Courses</span>
                    </router-link>
                </li>
                <li>
                    <div class="expandable" @click="toggleSection('courses')">
                        <span class="icon">📚</span>
                        <span v-show="isExpanded">Course List</span>
                    </div>
                    <ul v-if="sections.courses && isExpanded">
                        <li><router-link to="/course-list/1">In Person</router-link></li>
                        <li><router-link to="/course-list/2">Online</router-link></li>
                        <li><router-link to="/course-list/3">Archived Webinars</router-link></li>
                        <li><router-link to="/course-list/4">Live Webinars</router-link></li>
                        <li><router-link to="/course-list/5">Hybrid</router-link></li>
                        <li><router-link to="/course-list/6">New</router-link></li>
                    </ul>
                </li>
                <li>
                    <router-link to="/peer-certification">
                        <span class="icon">🏅</span>
                        <span v-show="isExpanded">Peer Certification</span>
                    </router-link>
                </li>
                <li>
                    <router-link to="/trending">
                        <span class="icon">🔥</span>
                        <span v-show="isExpanded">Trending</span>
                    </router-link>
                </li>

                <!-- ✅ Show only if user is logged in -->
                <li v-if="isUserLoggedIn">
                    <div class="expandable" @click="toggleSection('myCourses')">
                        <span class="icon">📋</span>
                        <span v-show="isExpanded">My Courses</span>
                    </div>
                    <ul v-if="sections.myCourses && isExpanded">
                        <li><router-link to="/my-courses/in-progress">In Progress</router-link></li>
                        <li><router-link to="/my-courses/registered">Registered</router-link></li>
                        <li><router-link to="/my-courses/completed">Completed</router-link></li>
                    </ul>
                </li>

                <!-- ✅ Show only if user is logged in -->
                <li v-if="isUserLoggedIn">
                    <router-link to="/my-certificates">
                        <span class="icon">📜</span>
                        <span v-show="isExpanded">My Certificates</span>
                    </router-link>
                </li>

                <li>
                    <router-link to="/course-module">
                        <span class="icon">📘</span>
                        <span v-show="isExpanded">Course Module</span>
                    </router-link>
                </li>

            </ul>
        </nav>
    </div>
</template>

<script>export default {
        name: "SideNav",
        data() {
            return {
                isExpanded: true,
                sections: {
                    courses: false,
                    myCourses: false,
                },
                isUserLoggedIn: !!localStorage.getItem("jwtToken"), // ✅ Check if user is logged in
            };
        },
        methods: {
            toggleSidenav() {
                this.isExpanded = !this.isExpanded;
            },
            toggleSection(section) {
                this.sections[section] = !this.sections[section];
            },
            checkUserLogin() {
                this.isUserLoggedIn = !!localStorage.getItem("jwtToken"); // ✅ Update login state
            },
        },
        mounted() {
            this.checkUserLogin();
            window.addEventListener("storage", this.checkUserLogin); // ✅ Listen for login/logout updates
        },
        beforeUnmount() {
            window.removeEventListener("storage", this.checkUserLogin);
        },
    };</script>

<style scoped>
    /* General Container */
    .sidenav-container {
        display: flex;
        height: 100vh;
        background-color: #f8f9fa;
    }

    /* Hamburger Button */
    .hamburger-button {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 50px;
        height: 50px;
        background-color: #3f51b5;
        color: white;
        font-size: 24px;
        cursor: pointer;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
        border-radius: 8px;
        margin: 10px;
        z-index: 1000;
    }

    /* Side Navigation */
    nav {
        width: 250px;
        background: linear-gradient(135deg, #3f51b5, #6e7be4);
        color: white;
        overflow-y: auto;
        transition: width 0.3s ease-in-out;
        border-radius: 10px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
    }

        nav.collapsed {
            width: 60px;
        }

    /* Navigation Items */
    .nav-items {
        list-style-type: none;
        padding: 0;
        margin: 0;
    }

        .nav-items li {
            display: flex;
            align-items: center;
            padding: 10px 15px;
            cursor: pointer;
            transition: background 0.3s ease;
        }

            .nav-items li:hover {
                background-color: rgba(255, 255, 255, 0.2);
            }

        .nav-items a {
            display: flex;
            align-items: center;
            gap: 10px;
            text-decoration: none;
            color: white;
            width: 100%;
            font-size: 16px;
        }

    .icon {
        font-size: 20px;
    }

    /* Expandable Sections */
    .expandable {
        display: flex;
        align-items: center;
        gap: 10px;
        cursor: pointer;
    }

    ul ul {
        padding-left: 20px;
        list-style: none;
    }

    /* Profile Section */
    .profile-section {
        padding: 15px;
        border-bottom: 1px solid rgba(255, 255, 255, 0.2);
    }

    .profile-header {
        display: flex;
        align-items: center;
        gap: 10px;
        font-weight: bold;
        font-size: 16px;
    }
</style>