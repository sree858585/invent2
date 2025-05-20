<template>
    <div class="mark-attendance-container">
        <h2>📝 Mark Attendance</h2>

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

        <div class="table-wrapper" v-if="courses.length">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Course Title</th>
                        <th>Training Center</th>
                        <th>Course Date</th>
                        <th>Region</th>
                        <th>Category</th>
                        <th>Format</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="course in courses" :key="course.courseSysId">
                        <td>{{ course.subjectTitle }}</td>
                        <td>{{ course.siteName }}</td>
                        <td>{{ formatDate(course.courseDate) }}</td>
                        <td>{{ course.regionLabel }}</td>
                        <td>{{ course.categoryLabel }}</td>
                        <td>{{ formatLookup(course.format, formats) }}</td>
                        <td><button class="btn-primary" @click="selectCourse(course)">Select</button></td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="pagination" v-if="totalPages > 1">
            <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">⏮ Prev</button>
            <span>Page {{ currentPage }} of {{ totalPages }}</span>
            <button @click="changePage(currentPage + 1)" :disabled="currentPage >= totalPages">Next ⏭</button>
        </div>

        <p v-else class="no-data">No courses found.</p>
    </div>
    <MarkAttendanceModal v-if="selectedCourse"
                         :course="selectedCourse"
                         @close="selectedCourse = null" />
</template>

<script setup>import { ref, onMounted, computed } from 'vue';
    import apiClient from '@/axios';
    import MarkAttendanceModal from '@/components/Modals/MarkAttendanceModal.vue';

    const selectedCourse = ref(null);
    const selectCourse = (course) => {
        selectedCourse.value = course;
    };
    const courses = ref([]);
    const formats = ref([]);
    const regions = ref([]);
    const sites = ref([]);
    const categories = ref([]);
    const totalCourses = ref(0);
    const currentPage = ref(1);
    const pageSize = 10;
    
    const filters = ref({
        title: '',
        region: '',
        format: '',
        site: '',
        category: '',
        fromDate: '',
        toDate: ''
    });

    const totalPages = computed(() => Math.ceil(totalCourses.value / pageSize));

    const fetchCourses = async () => {
        try {
            const params = {
                page: currentPage.value,
                pageSize,
                ...filters.value,
                siteId: filters.value.site
            };
            const res = await apiClient.get('/CourseAdmin/paged', { params });
            courses.value = res.data?.data?.$values ?? [];
            totalCourses.value = res.data?.total ?? 0;
        } catch (err) {
            console.error('Failed to fetch courses:', err);
        }
    };

    const loadDropdowns = async () => {
        const [regionRes, siteRes, formatRes, catRes] = await Promise.all([
            apiClient.get('/Lookup/regions'),
            apiClient.get('/Lookup/sites'),
            apiClient.get('/Lookup/formats'),
            apiClient.get('/Lookup/categories')
        ]);
        regions.value = regionRes.data?.$values ?? [];
        sites.value = siteRes.data?.$values ?? [];
        formats.value = formatRes.data?.$values ?? [];
        categories.value = catRes.data?.$values ?? [];
    };

    const changePage = (page) => {
        if (page < 1 || page > totalPages.value) return;
        currentPage.value = page;
        fetchCourses();
    };

    const resetFilters = () => {
        filters.value = {
            title: '',
            region: '',
            format: '',
            site: '',
            category: '',
            fromDate: '',
            toDate: ''
        };
        currentPage.value = 1;
        fetchCourses();
    };

    const formatDate = (date) => new Date(date).toLocaleDateString('en-US');

    const formatLookup = (code, list) => {
        const item = list.find(f => f.code === code);
        return item ? item.value : 'N/A';
    };

    onMounted(() => {
        loadDropdowns();
        fetchCourses();
    });</script>

<style scoped>
    .mark-attendance-container {
        padding: 30px;
        font-family: 'Segoe UI', sans-serif;
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
        }

    .date-group {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        align-items: center;
    }

    .btn-search {
        background-color: #1976d2;
        color: white;
        padding: 8px 16px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 8px 16px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }

    .table-wrapper {
        overflow-x: auto;
        margin-top: 20px;
    }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        background: #fff;
        border-radius: 10px;
        box-shadow: 0 0 8px rgba(0, 0, 0, 0.05);
    }

        .modern-table th,
        .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #ddd;
        }

        .modern-table th {
            background-color: #f0f0f0;
            font-weight: bold;
        }

    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 6px 14px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
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
                opacity: 0.5;
                cursor: not-allowed;
            }

    .no-data {
        text-align: center;
        margin-top: 40px;
        color: #777;
    }
</style>