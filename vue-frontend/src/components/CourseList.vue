<template>
    <div class="course-container">
        <!-- STICKY HEADER (top) -->
        <section class="courses-header">
            <div class="header-inner">
                <div class="search-box">
                    <input type="text" v-model="searchQuery" placeholder="Search courses..." />
                </div>

                <div class="filter-section advanced-filters">
                    <div class="filter-field">
                        <label>Region</label>
                        <select v-model="selectedRegion">
                            <option value="">All</option>
                            <option v-for="region in regionOptions" :key="region.code" :value="region.code">
                                {{ region.value }}
                            </option>
                        </select>
                    </div>

                    <div class="filter-field">
                        <label>Category</label>
                        <select v-model="selectedCategory">
                            <option value="">All</option>
                            <option v-for="category in categoryOptions" :key="category.code" :value="category.code">
                                {{ category.value }}
                            </option>
                        </select>
                    </div>

                    <div class="filter-field">
                        <label>Site</label>
                        <select v-model="selectedSite">
                            <option value="">All</option>
                            <option v-for="site in siteOptions" :key="site.siteSysId" :value="site.siteSysId">
                                {{ site.siteName }}
                            </option>
                        </select>
                    </div>

                    <div class="filter-field">
                        <label>From Date</label>
                        <input type="date" v-model="fromDate" />
                    </div>

                    <div class="filter-field">
                        <label>To Date</label>
                        <input type="date" v-model="toDate" />
                    </div>

                    <div class="filter-field reset-field">
                        <button @click="resetFilters">Reset</button>
                    </div>
                </div>
            </div>
        </section>

        <!-- SCROLLABLE CONTENT -->
        <section class="courses-scroll">
            <div v-if="loading" class="loading">Loading courses...</div>

            <template v-else>
                <div v-if="courses.length > 0" class="course-grid">
                    <div v-for="course in courses"
                         :key="course.courseSysId"
                         class="card"
                         @click="openCourseModal(course)">
                        <div class="card-image" :style="courseImageStyle">
                            <div v-if="course.cnecredits || course.oasascredits" class="credit-tag">
                                {{ [course.cnecredits ? 'CNE' : '', course.oasascredits ? 'OASAS' : ''].filter(Boolean).join(' | ') }}
                            </div>
                        </div>

                        <div class="card-content">
                            <h5 class="card-title" :title="course.subjectTitle">
                                {{ truncateText(course.subjectTitle || 'Untitled Course', 90) }}
                            </h5>

                            <div class="card-datetime-block">
                                <p class="card-date"><strong>Date:</strong> {{ formatDate(course.courseDate) }}</p>
                                <div class="card-time-seats">
                                    <p class="card-time" :title="course.courseTime">
                                        <strong>Time:</strong>
                                        {{ truncateText(course.courseTime || 'N/A', 40) }}
                                    </p>
                                    <span class="card-seats">Seats: {{ course.maxSeats }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <p v-else class="no-data">No courses available for this format.</p>
            </template>

            <div v-if="totalPages > 1" class="pagination">
                <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">⏮ Prev</button>
                <button v-for="page in visiblePages"
                        :key="page"
                        :disabled="page === '...'"
                        @click="typeof page === 'number' && changePage(page)"
                        :class="{ active: page === currentPage, ellipsis: page === '...' }">
                    {{ page }}
                </button>
                <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">Next ⏭</button>
            </div>
        </section>
    </div>

    <!-- Modals (outside scroll area) -->
    <CourseDetailModal v-if="selectedCourse"
                       :course="selectedCourse"
                       :formatLookup="formatLookup"
                       :categoryLookup="categoryLookup"
                       :regionLookup="regionLookup"
                       @register="handleRegister"
                       @request-login="showLoginModal = true"
                       @close="selectedCourse = null" />

    <SuccessModal v-if="showSuccessModal"
                  :message="successMessage"
                  :email="user?.email || ''"
                  @close="handleSuccessClose" />

    <LoginComponent v-if="showLoginModal"
                    @login-success="handleLoginSuccess"
                    @close="showLoginModal = false"
                    @show-register="handleShowRegister" />

    <RegisterComponent v-if="showRegisterModal"
                       @close="showRegisterModal = false"
                       @register-success="handleRegisterSuccess" />
</template>

<script>import apiClient from "@/axios";
import CourseDetailModal from "@/components/Modals/CourseDetailModal.vue";
import SuccessModal from "@/components/Modals/SuccessModal.vue";
import LoginComponent from "@/components/LoginComponent.vue";
import RegisterComponent from "@/components/RegistrationModal.vue";

export default {
  components: {
    CourseDetailModal,
    SuccessModal,
    LoginComponent,
    RegisterComponent,
  },
  props: [],
  data() {
    return {

      selectedFormats: ["all"], // strings: ["1","2"] or ["all"]

      courses: [],
      loading: true,
      currentPage: 1,
      pageSize: 9,
      totalItems: 0,
      selectedCourse: null,

      searchQuery: "",
      selectedRegion: "",
      selectedCategory: "",
      selectedSite: "",
      fromDate: "",
      toDate: "",

      regionOptions: [],
      categoryOptions: [],
      siteOptions: [],

      user: null,
      showSuccessModal: false,
      showLoginModal: false,
      showRegisterModal: false,
    };
  },
  computed: {
    courseImageStyle() {
      const imageUrl = require("@/assets/hiv2.png");
      return {
        backgroundImage: `url(${imageUrl})`,
        backgroundSize: "cover",
        backgroundPosition: "center",
        backgroundRepeat: "no-repeat",
      };
    },
    totalPages() {
      return Math.ceil(this.totalItems / this.pageSize);
    },
    visiblePages() {
      const pages = [];
      const total = this.totalPages;
      const current = this.currentPage;
      if (total <= 7) {
        for (let i = 1; i <= total; i++) pages.push(i);
      } else {
        pages.push(1);
        if (current > 3) pages.push("...");
        const start = Math.max(2, current - 1);
        const end = Math.min(total - 1, current + 1);
        for (let i = start; i <= end; i++) pages.push(i);
        if (current < total - 2) pages.push("...");
        pages.push(total);
      }
      return pages;
    },
  },
  watch: {
    "$route.query.formats": {
      immediate: true,
      handler(q) {
        const parts =
          typeof q === "string" && q.trim()
            ? q.split(",").map((s) => s.trim()).filter(Boolean)
            : ["all"];
        this.selectedFormats = parts.length ? parts : ["all"];
        this.currentPage = 1;
        this.getCourses();
      },
    },

    // other filters trigger reloads
    searchQuery() {
      this.currentPage = 1;
      this.getCourses();
    },
    selectedRegion() {
      this.currentPage = 1;
      this.getCourses();
    },
    selectedCategory() {
      this.currentPage = 1;
      this.getCourses();
    },
    selectedSite() {
      this.currentPage = 1;
      this.getCourses();
    },
    fromDate() {
      this.currentPage = 1;
      this.getCourses();
    },
    toDate() {
      this.currentPage = 1;
      this.getCourses();
    },
  },
  mounted() {
    this.loadLookups();
    this.fetchUser();

    // Normalize path to /course-list/all (keeps query intact)
    if (this.$route.path.startsWith("/course-list") && !this.$route.params.format) {
      this.$router.replace({ path: "/course-list/all", query: this.$route.query });
    }
  },
  methods: {
    handleShowRegister() {
      this.showLoginModal = false;
      this.showRegisterModal = true;
    },
    handleRegisterSuccess() {
      this.showRegisterModal = false;
      this.fetchUser();
    },
    handleLoginSuccess(userData) {
      localStorage.setItem("userId", userData.userId);
      localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
      localStorage.setItem("jwtToken", userData.token);
      this.showLoginModal = false;

      if (this.selectedCourse) {
        this.handleRegister(this.selectedCourse, true);
      }
    },
    async fetchUser() {
      const userId = localStorage.getItem("userId");
      if (!userId) return;
      try {
        const res = await apiClient.get(`/user/${userId}`);
        this.user = res.data;
      } catch (err) {
        console.error("Failed to fetch user:", err);
      }
    },
    async handleRegister(course, isFromLogin = false) {
      try {
        const userId = localStorage.getItem("userId");
        const res = await apiClient.post("/Course/register", {
          userId,
          courseId: course.courseSysId,
          adaneed: course.adaneed || false,
          adadetails: course.adadetails || "",
        });
        const msg = res.data?.message || "Registration successful.";
        this.successMessage = msg;
        this.showSuccessModal = true;

        if (!isFromLogin) {
          this.selectedCourse = null;
        } else {
          setTimeout(() => {
            this.selectedCourse = null;
          }, 500);
        }
      } catch (err) {
        console.error("Registration failed:", err);
      }
    },
    handleSuccessClose() {
      this.showSuccessModal = false;
    },

    resetFilters() {
      this.selectedRegion = "";
      this.selectedCategory = "";
      this.selectedSite = "";
      this.fromDate = "";
      this.toDate = "";
      this.searchQuery = "";
      this.getCourses();
    },

    async loadLookups() {
      try {
        const [regions, categories, sites] = await Promise.all([
          apiClient.get("/Lookup/regions"),
          apiClient.get("/Lookup/categories"),
          apiClient.get("/Lookup/sites"),
        ]);
        this.regionOptions = regions.data?.$values ?? regions.data ?? [];
        this.categoryOptions = categories.data?.$values ?? categories.data ?? [];
        this.siteOptions = sites.data?.$values ?? sites.data ?? [];
      } catch (error) {
        console.error("Error loading lookup data:", error);
      }
    },

    async getCourses() {
      this.loading = true;
      try {
        // If specific formats selected, pass them as comma-separated list
        const hasSpecific =
          !(this.selectedFormats.includes("all") || this.selectedFormats.length === 0);

        // Keep path param as 0 (All) to preserve your existing endpoint
        const formatParam = 0;
        // DEBUG
    console.log("getCourses() formats param:", hasSpecific ? this.selectedFormats.join(",") : undefined);

        const res = await apiClient.get(`/Course/FormatPaged/${formatParam}`, {
          params: {
            page: this.currentPage,
            pageSize: this.pageSize,
            search: this.searchQuery || undefined,
            region: this.selectedRegion || undefined,
            category: this.selectedCategory || undefined,
            site: this.selectedSite || undefined,
            fromDate: this.fromDate || undefined,
            toDate: this.toDate || undefined,
            // NEW: multi-select formats
            formats: hasSpecific ? this.selectedFormats.join(",") : undefined,
          },
        });

        this.courses = res.data?.data?.$values ?? [];
        this.totalItems = res.data?.total ?? 0;

        // ── Optional temporary client-side filter if backend isn't ready ──
        // if (hasSpecific) {
        //   const allowed = new Set(this.selectedFormats.map(Number));
        //   // TODO: replace 'trainingFormatId' with your real field name
        //   this.courses = this.courses.filter(c => allowed.has(c.trainingFormatId));
        //   this.totalItems = this.courses.length;
        // }
      } catch (error) {
        console.error("Error fetching courses:", error);
      } finally {
        this.loading = false;
      }
    },


    openCourseModal(course) {
      this.selectedCourse = course;
    },
    truncateText(text, maxLength) {
      return text?.length > maxLength ? text.slice(0, maxLength) + "..." : text;
    },
    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        this.getCourses();
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    },
    // if  still use this label somewhere in the template:
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
<!-- COMPONENT STYLES -->
<style scoped>
    /* === Layout variables === */
    .course-container {
        --header-offset: 0px; /* set to your fixed navbar height if you have one */
        display: grid;
        grid-template-rows: auto 1fr;
        min-height: 100vh;
        background: #f4f6f8;
        margin: 0;
        padding: 0;
    }

    /* === Sticky header at the top === */
    .courses-header {
        position: sticky;
        top: var(--header-offset);
        z-index: 30;
        background: linear-gradient(180deg,#ffffff 0%,#f6f8fb 100%);
        border-bottom: 1px solid #e5e7eb;
        box-shadow: 0 4px 12px rgba(0,0,0,0.04);
        margin: 0; /* no outer gap */
        padding: 0; /* keep spacing inside .header-inner */
        border-radius: 0;
    }

    /* spacing inside the header, not around it */
    .header-inner {
        padding: 12px 20px 14px;
    }

    /* === Scrollable grid section (no extra top gap) === */
    .courses-scroll {
        overflow: auto;
        padding: 12px 20px 24px; /* tiny space under the header only */
    }

    /* ====== Typography / helper ====== */
    .heading {
        text-align: center;
        font-size: 2.5rem;
        margin-bottom: 20px;
        color: #3f51b5;
    }

    .loading, .no-data {
        text-align: center;
        font-size: 1.5rem;
        color: #666;
        margin-top: 20px;
    }

    /* ====== Filters ====== */
    .filter-section {
        display: flex;
        flex-wrap: wrap;
        align-items: flex-end;
        justify-content: flex-start;
        gap: 20px;
    }

    .search-box {
        display: flex;
        flex-direction: column;
        flex: 1 1 220px;
        max-width: 260px;
        margin-bottom: 10px;
    }

        .search-box input {
            padding: 8px 12px;
            border: 1px solid #ccc;
            border-radius: 20px;
            font-size: 1rem;
            width: 100%;
            background: #fff;
        }

    .advanced-filters {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
        align-items: flex-end;
        justify-content: flex-start;
    }

    .filter-field {
        display: flex;
        flex-direction: column;
        flex: 1 1 180px;
        max-width: 240px;
    }

        .filter-field label {
            font-weight: 600;
            margin-bottom: 4px;
            color: #333;
        }

        .filter-field select,
        .filter-field input[type="date"] {
            padding: 8px 12px;
            border-radius: 20px;
            border: 1px solid #ccc;
            font-size: 1rem;
            background: #fff;
            color: #333;
            width: 100%;
            box-sizing: border-box;
        }

    .reset-field {
        display: flex;
        flex-direction: column;
        align-items: flex-end;
        justify-content: flex-end;
        flex: 1 1 140px;
        max-width: 160px;
    }

        .reset-field button {
            padding: 10px 24px;
            background: #e53935;
            color: #fff;
            border: none;
            border-radius: 24px;
            font-weight: bold;
            cursor: pointer;
            transition: background .3s ease;
            white-space: nowrap;
        }

            .reset-field button:hover {
                background: #c62828;
            }

    /* ====== Grid / Cards ====== */
    .course-grid {
        display: grid;
        grid-template-columns: repeat(3,1fr);
        gap: 32px;
        padding: 0;
    }

    @media (max-width:1024px) {
        .course-grid {
            grid-template-columns: repeat(2,1fr);
        }
    }

    @media (max-width:640px) {
        .course-grid {
            grid-template-columns: 1fr;
        }
    }

    .card {
        background: #fff;
        border-radius: 20px;
        overflow: hidden;
        box-shadow: 0 10px 25px rgba(0,0,0,.08);
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        transition: transform .25s ease;
        position: relative;
    }

        .card:hover {
            transform: translateY(-8px);
        }

    .card-image {
        height: 180px;
        width: 100%;
        background-size: cover;
        background-position: center;
        background-repeat: no-repeat;
        position: relative;
    }

    .credit-tag {
        position: absolute;
        top: 10px;
        right: 10px;
        background: #3f51b5;
        color: #fff;
        font-weight: bold;
        padding: 4px 10px;
        border-radius: 12px;
        font-size: .8rem;
        box-shadow: 0 2px 6px rgba(0,0,0,.1);
        z-index: 10;
    }

    .card-content {
        padding: 16px 20px 20px;
        display: flex;
        flex-direction: column;
        font-family: 'Segoe UI','Roboto',sans-serif;
    }

    .card-title {
        font-size: 1.2rem;
        font-weight: 700;
        color: #1a1a1a;
        display: -webkit-box;
        -webkit-line-clamp: 3;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        line-height: 1.4;
        height: calc(1.4em * 3);
        margin: 0;
        padding-top: 10px;
    }

    .card-datetime-block {
        display: flex;
        flex-direction: column;
        font-size: 1rem;
        color: #333;
        margin-top: 10px;
    }

    .card-time-seats {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 10px;
        margin-top: auto;
    }

    .card-time {
        font-weight: 500;
        color: #555;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        flex: 1;
    }

    .card-seats {
        background: #e8eaf6;
        color: #3f51b5;
        padding: 5px 10px;
        border-radius: 20px;
        font-weight: 600;
        font-size: .95rem;
        white-space: nowrap;
    }

    /* ====== Pagination ====== */
    .pagination {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 10px;
        margin-top: 24px;
        flex-wrap: wrap;
    }

        .pagination button {
            padding: 10px 16px;
            font-size: 15px;
            border-radius: 8px;
            border: 1px solid #ccc;
            background: #fff;
            color: #3f51b5;
            font-weight: 600;
            cursor: pointer;
            transition: all .3s ease;
        }

            .pagination button:hover:not(:disabled) {
                background: #3f51b5;
                color: #fff;
            }

            .pagination button:disabled {
                opacity: .5;
                cursor: not-allowed;
            }

        .pagination .active {
            background: #3f51b5;
            color: #fff;
        }

        .pagination .ellipsis {
            background: transparent;
            border: none;
            cursor: default;
            color: #999;
        }
</style>