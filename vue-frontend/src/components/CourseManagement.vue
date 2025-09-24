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
                        <th>
                            Course Date
                            <button class="sort-btn" @click="toggleSort('courseDate')">
                                <span v-if="sortField === 'courseDate' && sortOrder === 'asc'">▲</span>
                                <span v-else-if="sortField === 'courseDate' && sortOrder === 'desc'">▼</span>
                                <span v-else>⇅</span>
                            </button>
                        </th>
                        <th># Sign Up</th>
                        <th>
                            Delivered
                            <button class="sort-btn" @click="toggleSort('delivered')">
                                <span v-if="sortField === 'delivered' && sortOrder === 'asc'">▲</span>
                                <span v-else-if="sortField === 'delivered' && sortOrder === 'desc'">▼</span>
                                <span v-else>⇅</span>
                            </button>
                        </th>
                        <th>Course Management</th>
                    </tr>
                </thead>
                <tbody>
                        <tr v-for="course in sortedCourses"
                        :key="course.courseSysId"
                        :class="{'cancelled-row': course.cancelled === true}"
                        :title="course.cancelled ? 'This course is cancelled' : ''">
                            <td>
                                <a href="#"
                                   class="link-highlight"
                                   @click.prevent="showCourseDetails(course)"
                                   :class="{'strike': course.cancelled === true}">
                                    {{ course.subjectTitle }}
                                </a>

                                <!-- existing cancelled pill -->
                                <span v-if="course.cancelled" class="pill-cancelled">Cancelled</span>

                                <!-- new sticker row -->
                                <div class="sticker-row">
                                    <span v-if="course.hasWaitlist"
                                          class="sticker sticker-waitlist"
                                          title="This course currently has a waitlist">
                                        ⏳ Waitlist
                                    </span>

                                    <span v-if="course.hasAda"
                                          class="sticker sticker-ada"
                                          title="One or more attendees requested ADA services">
                                        ♿ ADA
                                    </span>
                                </div>
                            </td>
                        <td>{{ course.siteName }}</td>
                        <td>{{ formatDate(course.courseDate) }}</td>
                        <td>{{ formatSignups(course) }}</td>
                            <td>{{ computedDelivered(course) ? 'Yes' : 'No' }}</td>
                        <td>
                            <div class="dropdown">
                                <button class="dropdown-toggle"
                                        @click.stop="toggleDropdown(course.courseSysId, $event)"
                                        :class="{'cancelled-btn': course.cancelled === true}"
                                        :title="course.cancelled ? 'This course is cancelled - you can still manage it' : 'Manage course'">
                                    ⚙️ Manage
                                </button>
                            </div>
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
                <a @click.prevent="openModal('attendance', getCourseById(activeDropdownCourseId))">📝 Attendance Management</a>
                <div class="dropdown-approval">
                    <span>Approval:</span>
                    <label class="toggle-switch">
                        <input type="checkbox"
                               :checked="getCourseById(activeDropdownCourseId)?.approve"
                               @change="toggleApproval(getCourseById(activeDropdownCourseId))" />
                        <span class="slider"></span>
                    </label>
                </div>
                <a v-if="getCourseById(activeDropdownCourseId)?.cancelled"
                   @click.prevent="revertCancel(getCourseById(activeDropdownCourseId))">
                    ♻️ Revert Cancel
                </a>
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
                <p><strong>Delivered:</strong> {{ computedDelivered(selectedCourse) ? 'Yes' : 'No' }}</p>
                <p><strong>Approval:</strong> {{ selectedCourse.approve === true ? 'Yes' : (selectedCourse.approve === false ? 'No' : 'Pending') }}</p>
                <button class="btn-danger" @click="selectedCourse = null">Close</button>
            </div>
        </div>
    </div>
    <EditCourseModal v-if="modalType === 'edit'"
                     :course="modalCourse"
                     @close="closeModal"
                     @updated="handleCourseUpdated" />
    <AddUserModal v-if="modalType === 'addUser'"
                  :course="modalCourse"
                  @close="closeModal"
                  @user-changed="updateSignupCount" />
    <CancelCourseModal v-if="modalType === 'cancel'"
                       :course="modalCourse"
                       @close="closeModal"
                       @cancel-success="handleCourseCancelled" />
    <DropUserModal v-if="modalType === 'dropUser'"
                   :course="modalCourse"
                   @close="closeModal"
                   @user-changed="updateSignupCount" />
    <EmailUserModal v-if="modalType === 'email'" :course="modalCourse" @close="closeModal" />
    <MarkAttendanceModal v-if="modalType === 'attendance'"
                         :course="modalCourse"
                         @close="closeModal" />
</template>

<script>import apiClient from "@/axios.js";
    import ScheduleCourseModal from "@/components/ScheduleCourseModal.vue";
    import EditCourseModal from "@/components/Modals/EditCourseModal.vue";
    import AddUserModal from "@/components/Modals/AddUserModal.vue";
    import CancelCourseModal from "@/components/Modals/CancelCourseModal.vue";
    import DropUserModal from "@/components/Modals/DropUserModal.vue";
    import EmailUserModal from "@/components/Modals/EmailUsersModal.vue";
    import MarkAttendanceModal from "@/components/Modals/MarkAttendanceModal.vue";


    export default {
        name: "CourseManagement",
        components: {
            ScheduleCourseModal,
            EditCourseModal,
            AddUserModal,
            CancelCourseModal,
            DropUserModal,
            EmailUserModal,
            MarkAttendanceModal},
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
                sortField: null,
                sortOrder: null,
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
  sortedCourses() {
    const arr = [...this.courses];
    if (this.sortField) {
      arr.sort((a, b) => {
        let aVal, bVal;

        if (this.sortField === 'courseDate') {
          aVal = new Date(a.courseDate);
          bVal = new Date(b.courseDate);
        } else if (this.sortField === 'delivered') {
          aVal = this.computedDelivered(a) ? 1 : 0;
          bVal = this.computedDelivered(b) ? 1 : 0;
        }

        if (aVal < bVal) return this.sortOrder === 'asc' ? -1 : 1;
        if (aVal > bVal) return this.sortOrder === 'asc' ? 1 : -1;
        return 0;
      });
    }
    return arr;
  },
  totalPages() {
    return Math.ceil(this.totalCourses / this.pageSize);
  }
},

methods: {
            toggleSort(field) {
    if (this.sortField === field) {
      // toggle asc -> desc -> clear
      if (this.sortOrder === 'asc') this.sortOrder = 'desc';
      else if (this.sortOrder === 'desc') {
        this.sortField = null;
        this.sortOrder = null;
      } else this.sortOrder = 'asc';
    } else {
      this.sortField = field;
      this.sortOrder = 'asc';
    }
  },
            handleCourseCancelled() {
  this.fetchCourses();
},
            handleCourseUpdated() {
  this.fetchCourses(); 
},
formatSignups(course) {
  const enrolled = Number.isFinite(course.enrolledCount ?? course.registeredCount)
    ? (course.enrolledCount ?? course.registeredCount) : 0;
  const waitlist = Number.isFinite(course.waitlistCount) ? course.waitlistCount : 0;
  const total = Number.isFinite(course.totalRegistrations)
    ? course.totalRegistrations
    : (enrolled + waitlist);

  const cap = (typeof course.maxSeats === 'number' && course.maxSeats > 0) ? course.maxSeats : null;

  // ✅ Show total/max (e.g., 3/2)
  if (cap) return `${total}/${cap}`;

  // No cap → just show total (optionally add WL)
  return waitlist > 0 ? `${total} (+${waitlist} WL)` : `${total}`;
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
            async updateSignupCount({ courseSysId }) {
  const c = this.courses.find(x => x.courseSysId === courseSysId);
  if (!c) return;

  try {
    const { data } = await apiClient.get('/CourseAdmin/counts', { params: { courseId: courseSysId } });

    // set all three so formatSignups can render accurately
    if (typeof data?.enrolledCount === 'number') c.registeredCount = data.enrolledCount;
    if (typeof data?.waitlistCount === 'number') c.waitlistCount = data.waitlistCount;
    if (typeof data?.totalRegistrations === 'number') c.totalRegistrations = data.totalRegistrations;
    if (typeof data?.hasWaitlist === 'boolean') c.hasWaitlist = data.hasWaitlist;
    if (typeof data?.maxSeats === 'number' || data?.maxSeats === null) c.maxSeats = data.maxSeats;
  } catch (e) {
    console.warn('⚠️ Failed to refresh counts', e);
  }
},
            
            isNearBottom(event) {
                const button = event.target.closest('.dropdown');
                const rect = button.getBoundingClientRect();
                return rect.bottom + 160 > window.innerHeight;
            },
            async revertCancel(course) {
  if (!course) return;
  const ok = confirm(`Revert cancellation for "${course.subjectTitle}"?`);
  if (!ok) return;

  try {
    await apiClient.post('/CourseAdmin/revert-cancel', { courseSysId: course.courseSysId });
    // update local row immediately for snappy UI
    course.cancelled = false;
    this.activeDropdownCourseId = null;
    this.dropdownStyle.display = 'none';
    // and/or re-fetch to stay in sync with server
    this.fetchCourses();
  } catch (err) {
    console.error('Failed to revert cancel:', err);
    alert('Could not revert cancellation. Please try again.');
  }
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
            },computedDelivered(course) {
    if (!course) return false;
    const endOrStart = course.endDate || course.courseDate;
    if (!endOrStart) return !!course.delivered;
    const isPast = new Date(endOrStart) < new Date();
    return course.delivered === true || isPast;
  },
  async maybePersistAutoDelivered(course) {
    const endOrStart = course.endDate || course.courseDate;
    if (!endOrStart) return;
    const isPast = new Date(endOrStart) < new Date();

    if (isPast && course.delivered !== true) {
      try {
        await apiClient.put(`/CourseAdmin/updateDelivered/${course.courseSysId}`, {
          ...course,
          delivered: true
        });
        course.delivered = true; // update local row
      } catch (e) {
        console.error('❌ Failed to auto-persist delivered:', e);
      }
    }
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
    .dropdown-approval {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 8px 12px;
        border-top: 1px solid #eee;
        font-size: 14px;
        color: #333;
    }

        .dropdown-approval span {
            margin-right: 10px;
        }
    .cancelled-row {
        background: #f5f5f5;
        opacity: 0.65;
    }

    .link-highlight.strike {
        text-decoration: line-through;
        color: #666;
    }

    .pill-cancelled {
        display: inline-block;
        margin-left: 8px;
        padding: 2px 8px;
        font-size: 12px;
        border-radius: 999px;
        background: #ececec;
        color: #555;
        border: 1px solid #ddd;
    }

    .cancelled-btn {
        background-color: #fcecec;
        border-color: #f5b5b5;
        color: #a94442;
    }
    .sort-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        margin-left: 6px;
        font-size: 12px;
        color: #555;
    }

        .sort-btn:hover {
            color: #000;
        }
    .sticker-row {
        display: flex;
        gap: 8px;
        margin-top: 6px;
        flex-wrap: wrap;
    }

    .sticker {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 4px 10px;
        font-size: 12px;
        font-weight: 600;
        border-radius: 999px;
        border: 1px solid rgba(0,0,0,0.06);
        backdrop-filter: blur(6px);
        box-shadow: 0 2px 8px rgba(0,0,0,0.06);
        letter-spacing: 0.2px;
    }

    .sticker-waitlist {
        color: #7a3e00;
        background: linear-gradient(180deg, #fff5e6, #ffe8c7);
        border-color: #ffd9a1;
    }

    .sticker-ada {
        color: #084c2e;
        background: linear-gradient(180deg, #eafff5, #d5f7ea);
        border-color: #bdeedc;
    }

    /* optional: subtle hover lift */
    .sticker:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    }
</style>
