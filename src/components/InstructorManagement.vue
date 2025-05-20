<template>
    <div class="training-title-container">
        <div class="header">
            <h2>🧑‍🏫 Instructors</h2>
            <button class="btn-primary" @click="isModalOpen = true">➕ Add New Instructor</button>
        </div>
        <div class="filter-panel">
            <div class="filter-group">
                <input v-model="filters.name"
                       placeholder="Search by Name..."
                       @input="applyFilters" />
                <select v-model="filters.siteSysId">
                    <option value="">All Training Centers</option>
                    <option v-for="site in sites" :key="site.siteSysId" :value="site.siteSysId">
                        {{ site.siteName }}
                    </option>
                </select>
                <button class="btn-search" @click="applyFilters">Search</button>
                <button class="btn-secondary" @click="resetFilters">Reset</button>
            </div>
        </div>
        <AddInstructorModal v-if="isModalOpen" @close="isModalOpen = false" @created="fetchInstructors" />

        <div class="table-wrapper" v-if="Array.isArray(instructors) && instructors.length">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Training Center</th>
                        <th>Actions</th>
                        <th>Active</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="i in instructors" :key="i.instructorSysId">
                        <td class="truncate-cell" :title="i.name">{{ i.name }}</td>
                        <td class="truncate-cell" :title="i.siteName">{{ i.siteName }}</td>
                        <td>
                            <button class="btn-action" @click="openEditModal(i)">✏️ Edit</button>
                            <button class="btn-danger" @click="openArchiveModal(i)">🗃 Archive</button>
                        </td>
                        <td>
                            <label class="toggle-switch">
                                <input type="checkbox" :checked="i.active" @change="toggleInstructorActive(i)" />
                                <span class="slider"></span>
                            </label>
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination" v-if="totalPages > 1">
                <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">⏮ Prev</button>
                <span>Page {{ currentPage }} of {{ totalPages }}</span>
                <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">Next ⏭</button>
            </div>
        </div>

        <p v-else class="no-data">No instructors found.</p>
        <EditInstructorModal v-if="editModalOpen"
                             :instructor="editInstructor"
                             @close="editModalOpen = false"
                             @updated="fetchInstructors" />
        <ArchiveInstructorModal v-if="archiveModalOpen"
                                :instructor="archiveInstructor"
                                @close="archiveModalOpen = false"
                                @archived="fetchInstructors" />
    </div>
</template>

<script>import AddInstructorModal from "@/components/Modals/AddInstructorModal.vue";
    import EditInstructorModal from "@/components/Modals/EditInstructorModal.vue";
    import ArchiveInstructorModal from "@/components/Modals/ArchiveInstructorModal.vue";
    import apiClient from "@/axios";

    export default {
        components: { AddInstructorModal, EditInstructorModal, ArchiveInstructorModal },
        data() {
            return {
                archiveModalOpen: false,
                archiveInstructor: null,
                editModalOpen: false,
                editInstructor: null,
                isModalOpen: false,
                instructors: [],
                currentPage: 1,
                pageSize: 10,
                totalItems: 0,
                filters: {
                    name: "",
                    siteSysId: ""
                },
                sites: []
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.totalItems / this.pageSize);
            }
        },
        mounted() {
            this.loadSites();
            this.fetchInstructors();
        },
        methods: {
            openArchiveModal(instructor) {
                this.archiveInstructor = instructor;
                this.archiveModalOpen = true;
            },
            openEditModal(instructor) {
                this.editInstructor = instructor;
                this.editModalOpen = true;
            },
            async toggleInstructorActive(instructor) {
                try {
                    const updated = { ...instructor, active: !instructor.active };
                    await apiClient.put(`/InstructorManagement/updateActive/${instructor.instructorSysId}`, updated);
                    instructor.active = updated.active;
                } catch (err) {
                    console.error("Error toggling instructor active state:", err);
                    alert("❌ Failed to update instructor status.");
                }
            },
            applyFilters() {
                this.currentPage = 1;
                this.fetchInstructors();
            },
            resetFilters() {
                this.filters.name = "";
                this.filters.siteSysId = "";
                this.currentPage = 1;
                this.fetchInstructors();
            },
            async loadSites() {
                const res = await apiClient.get("/Lookup/sites");
                this.sites = res.data?.$values ?? [];
            },
            async fetchInstructors() {
                const params = {
                    page: this.currentPage,
                    pageSize: this.pageSize,
                };

                if (this.filters.name.trim()) params.name = this.filters.name.trim();
                if (this.filters.siteSysId) params.siteSysId = this.filters.siteSysId;

                const res = await apiClient.get("/InstructorManagement/paged", { params });

                this.instructors = res.data?.data?.$values ?? [];
                this.totalItems = res.data?.total ?? 0;
            },
            changePage(page) {
                if (page >= 1 && page <= this.totalPages) {
                    this.currentPage = page;
                    this.fetchInstructors();
                }
            }
        }
    };</script>

<style scoped>
    .training-title-container {
        padding: 32px;
        font-family: "Segoe UI", sans-serif;
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
        transition: background-color 0.3s ease;
    }

        .btn-primary:hover {
            background-color: #388e3c;
        }

    .table-wrapper {
        overflow-x: auto;
        margin-top: 20px;
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
            font-size: 15px;
        }

        .modern-table th {
            background-color: #f8f9fa;
            font-weight: 600;
        }

    .btn-action {
        background-color: #007bff;
        color: white;
        border: none;
        padding: 6px 12px;
        border-radius: 6px;
        margin-right: 8px;
        cursor: pointer;
    }

    .btn-danger {
        background-color: #e74c3c;
        color: white;
        border: none;
        padding: 6px 12px;
        border-radius: 6px;
        cursor: pointer;
    }

    .no-data {
        text-align: center;
        margin-top: 40px;
        color: #777;
        font-size: 16px;
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

    .truncate-cell {
        max-width: 300px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
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

    .btn-search {
        background-color: #007bff;
        color: white;
        padding: 10px 18px;
        font-size: 14px;
        border: none;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-search:hover {
            background-color: #0056b3;
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

    input:checked + .slider {
        background-color: #4caf50;
    }

        input:checked + .slider:before {
            transform: translateX(24px);
        }
</style>