<template>
    <div class="sidenav-container">
        <!-- Side Navigation -->
        <nav :class="{ expanded: isExpanded, collapsed: !isExpanded }">
            <div class="profile-section">
                <!-- If user is logged in, show profile -->
                <div v-if="isUserLoggedIn" class="profile-header" @click="toggleProfileDropdown">
                    <span class="icon">👤</span>
                    <span>My Profile</span>
                    <span class="dropdown-arrow">▼</span>
                </div>

                <!-- If user is not logged in, show login button -->
                <button v-else class="login-btn" @click="$emit('show-login')">
                    <span class="icon">🔒</span>
                    <span>Login/Register</span>
                </button>

                <!-- Profile dropdown -->
                <ul v-if="showProfileDropdown && isUserLoggedIn">
                    <li>
                        <a href="#" @click.prevent="navigateToProfile">View Profile</a>
                    </li>
                    <li>
                        <a href="#" @click="handleLogout">Logout</a>
                    </li>
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

                <!-- Dropdown Items Template -->
                <li v-if="isUserLoggedIn">
                    <div class="dropdown-header" @click="toggleSection('myCourses')">
                        <span class="icon">📋</span>
                        <span v-show="isExpanded">My Courses</span>
                        <span class="dropdown-arrow" :class="{ rotated: sections.myCourses }">▼</span>
                    </div>
                    <ul v-if="sections.myCourses && isExpanded" class="dropdown-menu">
                        <li><router-link to="/my-courses/in-progress">In Progress</router-link></li>
                        <li><router-link to="/my-courses/registered">Registered</router-link></li>
                        <li><router-link to="/my-courses/completed">Completed</router-link></li>
                    </ul>
                </li>

                <li v-if="isUserLoggedIn">
                    <router-link to="/my-certificates">
                        <span class="icon">📜</span>
                        <span v-show="isExpanded">My Certificates</span>
                    </router-link>
                </li>
                <li>
                    <router-link to="/course-list-page">
                        <span class="icon">🗂️</span>
                        <span v-show="isExpanded">Courses</span>
                    </router-link>
                </li>

                <li>
                    <div class="dropdown-header" @click="toggleSection('courses')">
                        <span class="icon">📚</span>
                        <span v-show="isExpanded">Course List</span>
                        <span class="dropdown-arrow" :class="{ rotated: sections.courses }">▼</span>
                    </div>
                    <ul v-if="sections.courses && isExpanded" class="dropdown-menu">
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
                <li>
                    <router-link to="/training-centers">
                        <span class="icon">👥</span>
                        <span v-show="isExpanded">Training Centers</span>
                    </router-link>
                </li>

                <li>
                    <router-link to="/course-module">
                        <span class="icon">📘</span>
                        <span v-show="isExpanded">Course Module</span>
                    </router-link>
                </li>

                <li>
                    <div class="dropdown-header" @click="toggleSection('system')">
                        <span class="icon">⚙️</span>
                        <span v-show="isExpanded">System Management</span>
                        <span class="dropdown-arrow" :class="{ rotated: sections.system }">▼</span>
                    </div>
                    <ul v-if="sections.system && isExpanded" class="dropdown-menu">
                        <li><router-link to="/course-management">Course Management</router-link></li>
                        <li><router-link to="/system/training-title">Training Titles</router-link></li>
                        <li><router-link to="/system/training-center">Training Centers</router-link></li>
                        <li><router-link to="/system/instructor-management">Instructor Management</router-link></li>
                        <li><router-link to="/system/course-list">Course List</router-link></li>
                    </ul>
                </li>
            </ul>

            <!-- Hamburger Button -->
            <div class="hamburger-button" @click="toggleSidenav">
                <span>&#9776;</span>
            </div>
        </nav>
    </div>
</template>

<script>import eventBus from "@/eventBus.js";
    export default {
        name: "SideNav",
        data() {
            return {
                isExpanded: true,
                showProfileDropdown: false,
                sections: {
                    courses: false,
                    myCourses: false,
                    system: false 

                },
                isUserLoggedIn: !!localStorage.getItem("jwtToken"),
            };
        },
        mounted() {
            eventBus.on("auth-change", this.refreshLoginState);
        },
        beforeUnmount() {
            eventBus.off("auth-change", this.refreshLoginState);
        },
        methods: {
            toggleSidenav() {
                this.isExpanded = !this.isExpanded;
            },
            toggleSection(section) {
                this.sections[section] = !this.sections[section];
            },
            toggleProfileDropdown() {
                this.showProfileDropdown = !this.showProfileDropdown;
            },
            navigateToProfile() {
                const userId = localStorage.getItem("userId");
                if (!userId) {
                    alert("User ID not found. Please log in again.");
                    return;
                }
                this.$router.push(`/profile/view/${userId}`);
            },
            handleLogout() {
                localStorage.removeItem("jwtToken");
                localStorage.removeItem("userName");
                localStorage.removeItem("userId");
                this.isUserLoggedIn = false;
                this.$router.push("/home");
                window.location.reload();
            },
            refreshLoginState() {
                this.isUserLoggedIn = !!localStorage.getItem("jwtToken");
            },
        },
    };</script>

<style scoped>

        /* General Container */
          .sidenav-container {
                display: flex;
                height: 100vh;

        }

        /* Hamburger Button */
          .hamburger-button {
                position: absolute;
                top: 10px;
                right: 10px;
                display: flex;
                justify-content: center;
                align-items: center;
                width: 40px;
                height: 40px;
                background-color: #ffffff;
                color: #6e528d;
                font-size: 20px;
                cursor: pointer;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
                border-radius: 50%;
                z-index: 1000;

        }
        /* Side Navigation */
          nav {
                width: 280px;
                background: #6e528d;
                color: white;
                overflow-y: auto;
                transition: width 0.3s ease-in-out;
                border-radius: 10px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
                position: relative;
        }
            nav.collapsed {
                  width: 60px;
        }
        /* Profile Section */
          .profile-section {
                padding: 15px;
                border-bottom: 1px solid rgba(255, 255, 255, 0.2);
                background-color: rgba(255, 255, 255, 0.1);
                border-radius: 8px;

        }
          .profile-header {
                display: flex;
                align-items: center;
                gap: 10px;
                font-weight: bold;
                font-size: 18px; /* Slightly larger font */
                color: #ffffff; /* Keep header text white */
                cursor: pointer; /* Change cursor on hover */
                transition: background 0.3s ease; /* Smooth hover effect */
                padding: 10px; /* Add some padding */
                border-radius: 5px; /* Rounded corners */

        }
            .profile-header:hover {
                  background-color: rgba(255, 255, 255, 0.2); /* Background change on hover */

        }
    .dropdown-arrow.rotated {
        transform: rotate(180deg);
    }
    .dropdown-arrow {
        transition: transform 0.3s ease;
    }
          .profile-header:hover .dropdown-arrow {
                transform: rotate(180deg); /* Rotate arrow when hovered */

        }
    .dropdown-header {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 10px 15px;
        font-size: 16px;
        font-weight: normal;
        color: white;
        cursor: pointer;
        transition: background-color 0.3s ease;
        border-radius: 5px;
        width: 100%;
        box-sizing: border-box;
    }
        .dropdown-header span {
            display: flex;
            align-items: center;
        }

        .dropdown-header .dropdown-arrow {
            margin-left: auto;
        }
        .dropdown-header:hover {
            background-color: rgba(255, 255, 255, 0.2);
        }
    .dropdown-menu {
        list-style: none;
        margin: 0;
        padding: 8px 0;
        margin-left: 35px; /* Indent under parent */
        background-color: rgba(255, 255, 255, 0.1);
        border-left: 2px solid rgba(255, 255, 255, 0.2);
        border-radius: 6px;
        transition: all 0.3s ease;
    }

        .dropdown-menu li {
            padding: 8px 12px;
            font-size: 15px;
            color: white;
            border-radius: 4px;
            margin: 2px 0;
            transition: background-color 0.2s ease;
        }

            .dropdown-menu li:hover {
                background-color: rgba(255, 255, 255, 0.2);
            }

            .dropdown-menu li a {
                color: white;
                text-decoration: none;
                display: block;
                width: 100%;
            }
        /* Dropdown Menu */
          .profile-section ul {
                background-color: rgba(255, 255, 255, 0.1); /* Semi-transparent background */
                border-radius: 5px;
                list-style: none;
                padding: 10px 0;
                margin: 0;

        }
          .profile-section li {
                padding: 8px 15px;
                transition: background 0.2s; /* Smooth background change */
                cursor: pointer; /* Pointer cursor for dropdown items */
        }
            .profile-section li:hover {
                  background-color: rgba(255, 255, 255, 0.2); /* Light background on hover */

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
                padding: 0;
                margin-bottom: 5px;
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
    .login-btn {
        background-color: rgba(255, 255, 255, 0.15);
        color: white;
        border: none;
        padding: 10px 15px;
        width: 100%;
        text-align: left;
        font-size: 16px;
        font-weight: bold;
        border-radius: 5px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 10px;
    }

        .login-btn:hover {
            background-color: rgba(255, 255, 255, 0.25);
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
</style>