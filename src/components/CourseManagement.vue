<template>
    <div class="course-management-container">
        <div class="header">
            <h2>📚 Course Management</h2>
            <button class="btn-primary" @click="isModalOpen = true">
                ➕ Schedule a New Course
            </button>
        </div>

        <ScheduleCourseModal :isOpen="isModalOpen" @close="isModalOpen = false" @submit="handleNewCourse" />
        <!-- 🔍 Filters -->
        <div class="filter-panel">
            <div class="filter-group">
                <input v-model="filters.title" placeholder="Course Title" />
                <select v-model="filters.region">
                    <option value="">All Regions</option>
                    <option v-for="r in regions" :key="r.code" :value="r.code">{{ r.value }}</option>
                </select>
                <select v-model="filters.format">
                    <option value="">All Formats</option>
                    <option v-for="f in formats" :key="f.code" :value="f.code">{{ f.value }}</option>
                </select>
                <select v-model="filters.site">
                    <option value="">All Sites</option>
                    <option v-for="s in sites" :key="s.siteSysId" :value="s.siteSysId">{{ s.siteName }}</option>
                </select>
                <select v-model="filters.category">
                    <option value="">All Categories</option>
                    <option v-for="c in categories" :key="c.code" :value="c.code">{{ c.value }}</option>
                </select>
            </div>
            <div class="filter-group date-group">
                <label>From</label>
                <input type="date" v-model="filters.fromDate" />
                <label>To</label>
                <input type="date" v-model="filters.toDate" />
                <button class="btn-search" @click="fetchCourses">Search</button>
                <button @click="resetFilters" class="btn-secondary">Reset</button>
            </div>
        </div>

        <div class="table-wrapper" v-if="courses && courses.length > 0">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Course Title</th>
                        <th>Training Center</th>
                        <th>Course Date</th>
                        <th># Sign Up</th>
                        <th>Delivered</th>
                        <th>Actions</th>
                        <th>Approval</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="course in courses" :key="course.courseSysId">
                        <td>
                            <a href="#" class="link-highlight" @click.prevent="showCourseDetails(course)">
                                {{ course.subjectTitle }}
                            </a>
                        </td>
                        <td>{{ course.siteName }}</td>
                        <td>{{ formatDate(course.courseDate) }}</td>
                        <td>{{ course.signupCount ?? 0 }}</td>
                        <td>
                            <label class="toggle-switch">
                                <input type="checkbox" :checked="course.delivered" @change="updateDelivered(course)" />
                                <span class="slider"></span>
                            </label>
                        </td>
                        <td>
                            <div class="dropdown">
                                <button class="dropdown-toggle"
                                        @click.stop="toggleDropdown(course.courseSysId, $event)">
                                    ⚙️ Manage
                                </button>


                            </div>
                        </td>
                        <td>
                            <label class="toggle-switch">
                                <input type="checkbox" :checked="course.approve" @change="toggleApproval(course)" />
                                <span class="slider"></span>
                            </label>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div v-if="activeDropdownCourseId !== null"
                 class="dropdown-menu-portal"
                 :style="dropdownStyle"
                 @click.stop>
                <a @click.prevent="openModal('edit', getCourseById(activeDropdownCourseId))">✏️ Edit</a>
                <a @click.prevent="openModal('addUser', getCourseById(activeDropdownCourseId))">👥 Add User</a>
                <a @click.prevent="openModal('cancel', getCourseById(activeDropdownCourseId))">🚫 Cancel</a>
                <a @click.prevent="openModal('dropUser', getCourseById(activeDropdownCourseId))">📤 Drop User</a>
                <a @click.prevent="openModal('email', getCourseById(activeDropdownCourseId))">📧 Email</a>
            </div>
            <!-- ⏩ Pagination -->
            <div class="pagination" v-if="totalPages > 1">
                <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">⏮ Prev</button>
                <span>Page {{ currentPage }} of {{ totalPages }}</span>
                <button @click="changePage(currentPage + 1)" :disabled="currentPage >= totalPages">Next ⏭</button>
            </div>
        </div>

        <p v-else class="no-data">No courses found.</p>

        <!-- Detail Modal -->
        <div class="modal-overlay" v-if="selectedCourse && !modalType">
            <div class="modal">
                <h3>📘 View Course Details</h3>
                <p><strong>Course Title:</strong> {{ selectedCourse.subjectTitle }}</p>
                <p><strong>Training Center:</strong> {{ selectedCourse.siteName }}</p>
                <p><strong>Location:</strong> {{ selectedCourse.trainingLocation }}</p>
                <p><strong>Course Date:</strong> {{ formatDate(selectedCourse.courseDate) }}</p>
                <p><strong>Delivered:</strong> {{ selectedCourse.delivered ? 'Yes' : 'No' }}</p>
                <p><strong>Approval:</strong> {{ selectedCourse.approve === true ? 'Yes' : (selectedCourse.approve === false ? 'No' : 'Pending') }}</p>
                <button class="btn-danger" @click="selectedCourse = null">Close</button>
            </div>
        </div>
    </div>
    <EditCourseModal v-if="modalType === 'edit'"
                     :course="modalCourse"
                     @close="closeModal"
                     @updated="handleCourseUpdated" />
    <AddUserModal v-if="modalType === 'addUser'" :course="modalCourse" @close="closeModal" />
    <CancelCourseModal v-if="modalType === 'cancel'" :course="modalCourse" @close="closeModal" />
    <DropUserModal v-if="modalType === 'dropUser'" :course="modalCourse" @close="closeModal" />
    <EmailUserModal v-if="modalType === 'email'" :course="modalCourse" @close="closeModal" />
</template>

<script>import apiClient from "@/axios.js";
    import ScheduleCourseModal from "@/components/ScheduleCourseModal.vue";
    import EditCourseModal from "@/components/Modals/EditCourseModal.vue";
    import AddUserModal from "@/components/Modals/AddUserModal.vue";
    import CancelCourseModal from "@/components/Modals/CancelCourseModal.vue";
    import DropUserModal from "@/components/Modals/DropUserModal.vue";
    import EmailUserModal from "@/components/Modals/EmailUsersModal.vue";


    export default {
        name: "CourseManagement",
        components: {
            ScheduleCourseModal,
            EditCourseModal,
            AddUserModal,
            CancelCourseModal,
            DropUserModal,
            EmailUserModal },
        data() {
            return {
                isModalOpen: false,
                selectedCourse: null,
                modalType: null,
                courses: [],
                totalCourses: 0,
                currentPage: 1,
                pageSize: 10,

                filters: {
                    title: "",
                    region: "",
                    format: "",
                    site: "",
                    category: "",
                    fromDate: "",
                    toDate: ""
                },
                dropdownStyle: {
                    top: 0,
                    left: 0,
                    display: 'none'
                },
                activeDropdownCourseId: null,

                regions: [],
                formats: [],
                sites: [],
                categories: [],
                dropdownOpen: null,
                dropdownDirection: {}
            };
        },
        
        mounted() {
            window.addEventListener("click", this.handleClickOutside);
            this.fetchCourses();
            this.loadDropdowns();
        },
        beforeUnmount() {
            window.removeEventListener("click", this.handleClickOutside);
        },
        closeDropdown(event) {
            const isClickInsideDropdown = event.target.closest('.dropdown-menu-portal');
            const isClickOnToggle = event.target.closest('.dropdown-toggle');

            if (!isClickInsideDropdown && !isClickOnToggle) {
                this.activeDropdownCourseId = null;
                this.dropdownStyle.display = 'none';
            }
        },
        computed: {
            totalPages() {
                return Math.ceil(this.totalCourses / this.pageSize);
            }
        },
        methods: {
            handleCourseUpdated() {
  this.fetchCourses(); 
},
            handleClickOutside(event) {
                const dropdown = document.querySelector(".dropdown-menu-portal");
                const toggleButtons = document.querySelectorAll(".dropdown-toggle");

                const clickedInsideDropdown = dropdown?.contains(event.target);
                const clickedToggle = Array.from(toggleButtons).some(btn => btn.contains(event.target));

                if (!clickedInsideDropdown && !clickedToggle) {
                    this.activeDropdownCourseId = null;
                    this.dropdownStyle.display = "none";
                }
            },
            getCourseById(courseId) {
                return this.courses.find(c => c.courseSysId === courseId);
            },
            
            isNearBottom(event) {
                const button = event.target.closest('.dropdown');
                const rect = button.getBoundingClientRect();
                return rect.bottom + 160 > window.innerHeight;
            },
            toggleDropdown(courseId, event) {
                if (this.activeDropdownCourseId === courseId) {
                    this.activeDropdownCourseId = null;
                    this.dropdownStyle.display = 'none';
                    return;
                }

                const button = event.currentTarget;
                const rect = button.getBoundingClientRect();

                const dropdownHeight = 160; // or dynamically calculated
                const spaceBelow = window.innerHeight - rect.bottom;
                const shouldDropUp = spaceBelow < dropdownHeight;

                this.dropdownStyle = {
                    top: `${shouldDropUp ? rect.top - dropdownHeight : rect.bottom}px`,
                    left: `${rect.left}px`,
                    display: 'block'
                };

                this.activeDropdownCourseId = courseId;
            },
            resetFilters() {
                this.filters = {
                    title: "",
                    region: "",
                    format: "",
                    site: "",
                    category: "",
                    fromDate: "",
                    toDate: ""
                };
                this.fetchCourses();
            },
            async loadDropdowns() {
                try {
                    const [regionRes, formatRes, siteRes, catRes] = await Promise.all([
                        apiClient.get("/Lookup/regions"),
                        apiClient.get("/Lookup/formats"),
                        apiClient.get("/Lookup/sites"),
                        apiClient.get("/Lookup/categories")
                    ]);

                    this.regions = regionRes.data?.$values ?? [];
                    this.formats = formatRes.data?.$values ?? [];
                    this.sites = siteRes.data?.$values ?? [];
                    this.categories = catRes.data?.$values ?? [];
                    console.log("Regions:", this.regions);
                    console.log("Formats:", this.formats);
                    console.log("Sites:", this.sites);
                    console.log("Categories:", this.categories);
                } catch (error) {
                    console.error("❌ Failed to load dropdowns:", error);
                }
            },
            async fetchCourses() {
                try {
                    const {
                        title, region, format, site, category, fromDate, toDate
                    } = this.filters;

                    const params = {
                        page: this.currentPage,
                        pageSize: this.pageSize
                    };

                    if (title?.trim()) params.title = title.trim();
                    if (region) params.region = region;
                    if (format) params.format = format;
                    if (site) params.siteId = site;
                    if (category) params.category = category;
                    if (fromDate) params.fromDate = fromDate;
                    if (toDate) params.toDate = toDate;

                    const res = await apiClient.get("/CourseAdmin/paged", { params });

                    this.courses = res.data?.data?.$values || [];
                    this.totalCourses = res.data?.total || 0;

                } catch (err) {
                    console.error("❌ Failed to fetch courses:", err);
                }
            },
            toggleApproval(course) {
                const newValue = !course.approve;
                this.updateApproval(course, newValue);
            },
            changePage(page) {
                if (page < 1 || page > this.totalPages) return;
                this.currentPage = page;
                this.fetchCourses();
            },
            handleNewCourse(courseData) {
                console.log("New Course Scheduled:", courseData);
            },
            openModal(type, course) {
                this.modalType = type;
                this.modalCourse = course;
                this.selectedCourse = null;
                this.dropdownOpen = null;
            },
            closeModal() {
                this.modalType = null;
                this.modalCourse = null;
            },
            async updateDelivered(course) {
                try {
                    const updatedCourse = { ...course, delivered: !course.delivered };
                    await apiClient.put(`/CourseAdmin/updateDelivered/${course.courseSysId}`, updatedCourse);
                    course.delivered = updatedCourse.delivered;
                } catch (error) {
                    console.error("Failed to update delivered status:", error);
                    alert("Error updating delivered status.");
                }
            },

            async updateApproval(course, value) {
                try {
                    const updatedCourse = { ...course, approve: value };
                    await apiClient.put(`/CourseAdmin/updateApproval/${course.courseSysId}`, updatedCourse);
                    course.approve = value;
                } catch (error) {
                    console.error("Failed to update approval status:", error);
                    alert("Error updating approval status.");
                }
            },
            formatDate(dateStr) {
                if (!dateStr) return "";
                const date = new Date(dateStr);
                return date.toLocaleDateString("en-US");
            },
            showCourseDetails(course) {
                this.selectedCourse = course;
            },
        },
    };</script>

<style scoped>
    .course-management-container {
        padding: 20px 40px;
        font-family: 'Segoe UI', sans-serif;
        color: #333;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 24px;
    }

        .header h2 {
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .btn-primary {
        background-color: #4caf50;
        color: white;
        border: none;
        padding: 10px 20px;
        font-size: 16px;
        border-radius: 10px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-primary:hover {
            background-color: #3e8e41;
        }

    .filter-panel {
        background: #f9fafb;
        padding: 20px;
        border-radius: 16px;
        margin-bottom: 24px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
    }

    .filter-group {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 16px;
        margin-bottom: 16px;
    }

        .filter-group input,
        .filter-group select {
            padding: 10px 14px;
            border: 1px solid #ccc;
            border-radius: 12px;
            font-size: 14px;
            background: #fff;
            transition: border 0.3s ease;
        }

            .filter-group input:focus,
            .filter-group select:focus {
                border-color: #4caf50;
                outline: none;
            }

    .date-group {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        align-items: center;
        margin-top: 12px;
    }

        .date-group label {
            font-size: 14px;
            font-weight: 500;
        }

    .btn-search {
        background-color: #007bff;
        color: white;
        border: none;
        padding: 10px 18px;
        font-size: 14px;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-search:hover {
            background-color: #0056b3;
        }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        border: none;
        padding: 10px 18px;
        font-size: 14px;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-secondary:hover {
            background-color: #cfcfcf;
        }

    .table-wrapper {
        overflow: visible !important;
        position: relative;
        z-index: 0;
    }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        background-color: #fff;
        border-radius: 10px;
        overflow: hidden;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
    }

        .modern-table th,
        .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left;
        }

        .modern-table th {
            background-color: #f8f9fa;
            font-weight: 600;
            white-space: nowrap;
        }

    .link-highlight {
        color: #007bff;
        font-weight: 500;
        cursor: pointer;
    }

        .link-highlight:hover {
            text-decoration: underline;
        }

    .no-data {
        text-align: center;
        margin-top: 40px;
        color: #777;
    }

    .dropdown {
        position: relative;
        display: inline-block;
    }

    .dropdown-toggle {
        background-color: #f1f1f1;
        border: 1px solid #ccc;
        border-radius: 6px;
        padding: 6px 10px;
        font-size: 14px;
        cursor: pointer;
    }

    .dropdown-menu {
        display: none;
        position: absolute;
        background-color: white;
        min-width: 160px;
        border: 1px solid #ddd;
        border-radius: 6px;
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
        z-index: 1000;
        margin-top: 4px;
    }

    .dropdown-menu a {
        padding: 10px 12px;
        display: block;
        text-decoration: none;
        color: #333;
        font-size: 14px;
    }

        .dropdown-menu a:hover {
            background-color: #f2f2f2;
        }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.65);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal {
        background: white;
        padding: 30px;
        border-radius: 12px;
        width: 550px;
        max-height: 85vh;
        overflow-y: auto;
        box-shadow: 0 12px 30px rgba(0, 0, 0, 0.2);
    }

        .modal h3 {
            font-size: 20px;
            margin-bottom: 16px;
        }

    .btn-danger {
        background-color: #e74c3c;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 6px;
        margin-top: 20px;
        cursor: pointer;
    }

        .btn-danger:hover {
            background-color: #c0392b;
        }

    .pagination {
        margin-top: 20px;
        text-align: center;
    }

        .pagination button {
            background: #f1f1f1;
            border: 1px solid #ccc;
            padding: 6px 12px;
            margin: 0 4px;
            border-radius: 6px;
            cursor: pointer;
        }

            .pagination button:disabled {
                cursor: not-allowed;
                opacity: 0.5;
            }

    .toggle-switch {
        position: relative;
        display: inline-block;
        width: 50px;
        height: 26px;
    }

        .toggle-switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    .slider {
        position: absolute;
        cursor: pointer;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: #ccc;
        transition: .4s;
        border-radius: 26px;
    }

        .slider:before {
            position: absolute;
            content: "";
            height: 20px;
            width: 20px;
            left: 3px;
            bottom: 3px;
            background-color: white;
            transition: .4s;
            border-radius: 50%;
        }

    .toggle-switch input:checked + .slider {
        background-color: #4caf50;
    }

        .toggle-switch input:checked + .slider:before {
            transform: translateX(24px);
        }
    .dropdown {
        position: relative;
        display: inline-block;
    }

    .dropdown-toggle {
        background-color: #f1f1f1;
        border: 1px solid #ccc;
        border-radius: 6px;
        padding: 6px 10px;
        font-size: 14px;
        cursor: pointer;
    }

    .dropdown-menu {
        position: absolute;
        background-color: white;
        min-width: 160px;
        border: 1px solid #ddd;
        border-radius: 6px;
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
        z-index: 2000;
        left: 0;
        display: block;
    }

        .dropdown-menu.drop-up {
            bottom: 100%;
            margin-bottom: 4px;
        }

        .dropdown-menu:not(.drop-up) {
            top: 100%;
            margin-top: 4px;
        }

        .dropdown-menu.drop-up {
            top: auto;
            bottom: 100%;
            margin-top: 0;
            margin-bottom: 4px;
        }

        .dropdown-menu a {
            padding: 10px 12px;
            display: block;
            text-decoration: none;
            color: #333;
            font-size: 14px;
        }

            .dropdown-menu a:hover {
                background-color: #f2f2f2;
            }
    .dropdown-menu-portal {
        position: fixed;
        background-color: white;
        border: 1px solid #ddd;
        border-radius: 6px;
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
        z-index: 9999;
        min-width: 160px;
    }

        .dropdown-menu-portal a {
            display: block;
            padding: 10px 12px;
            font-size: 14px;
            color: #333;
            text-decoration: none;
        }

            .dropdown-menu-portal a:hover {
                background-color: #f2f2f2;
            }
</style>
