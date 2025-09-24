<template>
    <div class="sidenav-container">
        <!-- Side Navigation -->
        <nav :class="{ expanded: isExpanded, collapsed: !isExpanded }">
            <div class="sidenav-scrollable">
                <!-- User Dashboard Section -->
                <div v-if="isUserLoggedIn && isStandardUser" class="dropdown-header" @click="toggleSection('user')">
                    <span class="icon">👤</span>
                    <span v-show="isExpanded">My Dashboard</span>
                    <span class="dropdown-arrow" :class="{ rotated: sections.user }">▼</span>
                </div>
                <ul v-if="sections.user && isExpanded" class="dropdown-menu">
                    <li><router-link to="/my-courses/registered">📋 My Learnings</router-link></li>
                    <li><router-link to="/my-certificates">📜 My Certificates</router-link></li>
                    <li><a href="#" @click.prevent="navigateToProfile">👤 View Profile</a></li>
                    <li><a href="#" @click="handleLogout">🔓 Logout</a></li>
                </ul>

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
                            <span v-show="isExpanded">Upcoming Courses</span>
                        </router-link>
                    </li>

                    <li>
                        <div class="dropdown-header" @click="goToCourses">
                            <span class="icon">📚</span>
                            <span v-show="isExpanded">Courses</span>
                            <span class="dropdown-arrow" :class="{ rotated: sections.courses }">▼</span>
                        </div>

                        <!-- Dropdown with checkboxes -->
                        <div v-if="sections.courses && isExpanded" class="dropdown-menu">
                            <!-- Multi-select checkboxes -->
                            <div class="format-filter">

                                <div class="format-checkbox">
                                    <input type="checkbox" id="fmt-all"
                                           :checked="isAllSelected"
                                           @change="toggleAll($event.target.checked)" />
                                    <label for="fmt-all">All</label>
                                </div>

                                <div class="format-checkbox" v-for="fmt in formatOptions" :key="fmt.id">
                                    <input type="checkbox"
                                           :id="`fmt-${fmt.id}`"
                                           :value="fmt.id"
                                           :checked="selectedFormats.includes(String(fmt.id))"
                                           @change="toggleFormat(fmt.id, $event.target.checked)" />
                                    <label :for="`fmt-${fmt.id}`">{{ fmt.label }}</label>
                                </div>

                                
                            </div>
                        </div>
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

                    <!-- ✅ SYSTEM MANAGEMENT: Only for Admin/Manager -->
                    <li v-if="isUserLoggedIn && isAdminOrManager">
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

                    <!-- ✅ ATTENDANCE MANAGEMENT: Only for Admin/Manager -->
                    <!--<li v-if="isUserLoggedIn && isAdminOrManager">
                        <div class="dropdown-header" @click="toggleSection('attendance')">
                            <span class="icon">📝</span>
                            <span v-show="isExpanded">Attendance Management</span>
                            <span class="dropdown-arrow" :class="{ rotated: sections.attendance }">▼</span>
                        </div>
                        <ul v-if="sections.attendance && isExpanded" class="dropdown-menu">
                            <li><router-link to="/attendance/mark">Mark Attendance</router-link></li>
                            <li><router-link to="/attendance/view">View Attendance</router-link></li>
                        </ul>
                    </li>-->


                    <!-- ✅ ROLE MANAGEMENT: Only for Admin/Manager -->
                    <li v-if="isUserLoggedIn && isAdminOrManager">
                        <div class="dropdown-header" @click="toggleSection('roles')">
                            <span class="icon">🛡️</span>
                            <span v-show="isExpanded">Role Management</span>
                            <span class="dropdown-arrow" :class="{ rotated: sections.roles }">▼</span>
                        </div>
                        <ul v-if="sections.roles && isExpanded" class="dropdown-menu">
                            <li><router-link to="/role-management/admins">🧑‍💼 Admin Roles</router-link></li>
                            <li><router-link to="/role-management/managers">👔 Manager Roles</router-link></li>
                        </ul>
                    </li>

                    <!-- TEMP DEBUG: REMOVE AFTER VERIFICATION -->
                    <li style="color: yellow; font-size: 14px; padding: 10px;">
                        Role: {{ userRole }}
                    </li>

                </ul>
            </div>

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
                    system: false,
                    roles: false


                },
                userRole: localStorage.getItem("userRole") || "",
                isUserLoggedIn: !!localStorage.getItem("jwtToken"),
                // NEW: course format selections
                selectedFormats: ['all'],
                formatOptions: [
                    { id: 1, label: 'In Person' },
                    { id: 2, label: 'Online' },
                    { id: 3, label: 'Archived Webinars' },
                    { id: 4, label: 'Live Webinars' },
                    { id: 5, label: 'Hybrid' },
                    { id: 6, label: 'New' },
                ],
            };
        },
        computed: {
  isAdminOrManager() {
    return this.userRole === "Admin" || this.userRole === "Manager";
  },
  isStandardUser() {
    return this.userRole === "User";
            },
            // 'All' is considered selected if 'all' is in the array OR none are checked
            isAllSelected() {
                return this.selectedFormats.includes('all') || this.selectedFormats.length === 0;
            }
            
},
        
        mounted() {
            // Initialize from URL (so refresh keeps selections)
            const q = this.$route.query?.formats;
            if (typeof q === 'string' && q.trim()) {
                const parts = q.split(',').map(s => s.trim()).filter(Boolean);
                this.selectedFormats = parts.length ? parts : ['all'];
            }
            if (this.$route.path.startsWith('/course-list')) {
                this.sections.courses = true;
            }
            eventBus.on("auth-change", this.refreshLoginState);
        },
        beforeUnmount() {
            eventBus.off("auth-change", this.refreshLoginState);
        }, watch: {
            // Open the dropdown when you navigate TO any course-list route
            '$route.path'(val) {
                if (val.startsWith('/course-list')) {
                    this.sections.courses = true; // auto-open on arrival
                }
            }
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
            // Centralized: update the URL immediately (auto-apply)
            updateCourseRoute() {
                const formatsParam = this.isAllSelected ? undefined : this.selectedFormats.join(',');
                this.$router.push({
                    path: '/course-list/all',
                    query: formatsParam ? { formats: formatsParam } : {}
                });
            },
            // NEW: checkbox logic
            toggleAll(checked) {
                this.selectedFormats = checked ? ['all'] : [];
                this.updateCourseRoute(); // auto-apply

            },
            goToCourses() {
                const onCourses = this.$route.path.startsWith('/course-list');

                if (!onCourses) {
                    // First time / not on the page yet: navigate and open
                    this.selectedFormats = ['all'];
                    this.$router.push({ path: '/course-list/all' });
                    this.sections.courses = true;
                } else {
                    // Already on the page: just toggle open/close
                    this.sections.courses = !this.sections.courses;
                }
            },
            toggleFormat(id, checked) {
                // normalize to strings
                const key = String(id);

                // remove 'all' when picking specifics
                this.selectedFormats = this.selectedFormats.filter(v => v !== 'all');

                if (checked) {
                    if (!this.selectedFormats.includes(key)) this.selectedFormats.push(key);
                } else {
                    this.selectedFormats = this.selectedFormats.filter(v => v !== key);
                }

                // if none selected, fall back to 'all'
                if (this.selectedFormats.length === 0) this.selectedFormats = ['all'];

                this.updateCourseRoute(); // auto-apply
            },

            applyCourseFilters() {
                // Build query param `formats=1,2,4` or clear for 'all'
                const formatsParam = this.isAllSelected
                    ? undefined
                    : this.selectedFormats.join(',');

                // Always navigate to /course-list/all to keep your current route pattern,
                // but pass the comma-separated formats in the query string.
                this.$router.push({
                    path: '/course-list/all',
                    query: formatsParam ? { formats: formatsParam } : {}
                });
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
    this.userRole = localStorage.getItem("userRole") || "";
    this.$forceUpdate();
},
        },
    };</script>

<style scoped>
    /* ===== Container ===== */
    /* ===== Container ===== */
    .sidenav-container {
        height: 100vh;
        background: #f4f6f9;
        overflow: hidden; /* Prevent outer scroll bar */
        display: flex;
        border-radius: 0 10px 10px 0; /* Only top-left rounded */
    }

   

    /* ===== Hamburger Toggle ===== */
    .hamburger-button {
        position: fixed; /* fixed to the viewport */
        top: 16px;
        left: 16px;
        z-index: 1100;
        width: 42px;
        height: 42px;
        background-color: #ffffff;
        color: #6e528d;
        font-size: 22px;
        border-radius: 50%;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
        display: flex;
        justify-content: center;
        align-items: center;
        cursor: pointer;
    }

    /* ===== Sidebar Core ===== */
    nav {
        width: 260px;
        background: #6e528d;
        color: #ffffff;
        overflow-y: auto;
        transition: width 0.3s ease, margin-left 0.3s ease;
        box-shadow: 2px 0 10px rgba(0, 0, 0, 0.15);
        display: flex;
        flex-direction: column;
        position: relative;
        border-radius: 10px;
    }

        nav.collapsed {
            width: 70px;
        }

    /* ===== Profile Section ===== */
    .profile-section {
        padding: 16px;
        border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        background: rgba(255, 255, 255, 0.05);
    }

    .profile-header,
    .login-btn {
        display: flex;
        align-items: center;
        gap: 12px;
        font-size: 16px;
        font-weight: 500;
        color: #ffffff;
        padding: 12px;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.2s ease;
    }

        .profile-header:hover,
        .login-btn:hover {
            background: rgba(255, 255, 255, 0.15);
        }

    .profile-section ul {
        margin-top: 10px;
        padding-left: 12px;
        list-style: none;
    }

    .profile-section li {
        padding: 6px 0;
        font-size: 14px;
        color: #e0e0e0;
        cursor: pointer;
    }

        .profile-section li:hover {
            color: #ffffff;
        }

    /* ===== Navigation Items ===== */
    .nav-items {
        list-style: none;
        padding: 0;
        margin: 0;
        flex-grow: 1;
    }

        .nav-items li {
            margin-bottom: 4px;
        }

        .nav-items a,
        .dropdown-header {
            display: flex;
            align-items: center;
            gap: 14px;
            padding: 12px 20px;
            color: #ffffff;
            font-size: 15px;
            border-radius: 8px;
            text-decoration: none;
            transition: background 0.2s ease;
        }

            .nav-items a:hover,
            .dropdown-header:hover {
                background-color: rgba(255, 255, 255, 0.15);
            }

    .icon {
        font-size: 18px;
    }

    /* ===== Dropdown Sections ===== */
    .dropdown-header {
        cursor: pointer;
        font-weight: 500;
    }

    .dropdown-arrow {
        margin-left: auto;
        transition: transform 0.3s ease;
    }

        .dropdown-arrow.rotated {
            transform: rotate(180deg);
        }

    .dropdown-menu {
        list-style: none;
        padding-left: 32px;
        margin: 6px 0 12px;
    }

        .dropdown-menu li {
            margin: 4px 0;
        }

            .dropdown-menu li a {
                font-size: 14px;
                color: #e0e0e0;
                text-decoration: none;
                display: block;
                padding: 6px 0;
                transition: color 0.2s ease;
            }

                .dropdown-menu li a:hover {
                    color: #ffffff;
                }

    /* ===== Login Button ===== */
    .login-btn {
        background-color: rgba(255, 255, 255, 0.08);
        font-size: 15px;
        font-weight: 500;
        width: 100%;
        text-align: left;
        border: none;
    }

    /* ===== Responsive Behavior ===== */
    @media (max-width: 768px) {
        nav {
            position: absolute;
            z-index: 1000;
            height: 100%;
        }
    }
    .sidenav-scrollable {
        flex: 1;
        overflow-y: auto;
        max-height: 100vh;
        padding-bottom: 20px;
    }

    /* add inside <style scoped> in SideNav.vue    new css  */

    .format-filter {
        padding: 10px 0 10px 4px;
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .format-title {
        font-weight: 600;
        color: #fff;
        margin-bottom: 4px;
    }

    .format-checkbox {
        display: flex;
        align-items: center;
        gap: 8px;
        color: #e0e0e0;
    }

</style>