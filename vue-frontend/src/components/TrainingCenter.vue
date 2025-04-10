<template>
    <div class="system-page">
        <div class="header">
            <h2>🏢 Training Centers</h2>
            <button class="btn-primary" @click="isModalOpen = true">➕ Create New</button>
        </div>

        <div class="filter-panel">
            <div class="filter-group">
                <input v-model="filters.name" placeholder="Search by Name..." @input="applyFilters" />
                <input v-model="filters.zip" placeholder="Search by ZIP Code..." @input="applyFilters" />
                <button class="btn-search" @click="applyFilters">Search</button>
                <button class="btn-secondary" @click="resetFilters">Reset</button>
            </div>
        </div>

        <CreateTrainingCenterModal v-if="isModalOpen"
                                   @close="isModalOpen = false"
                                   @created="fetchCenters" />

        <div class="table-wrapper" v-if="centers.length">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Training Center Name</th>
                        <th>Short Name / Description</th>
                        <th>Active</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="center in centers" :key="center.siteSysId">
                        <td class="truncate-cell" :title="center.siteName">{{ center.siteName }}</td>
                        <td class="truncate-cell">
                            <strong>Short Name:</strong> {{ center.shortName || '—' }}<br />
                            <strong>Description:</strong> {{ center.description || '—' }}
                        </td>
                        <td>
                            <label class="toggle-switch">
                                <input type="checkbox"
                                       :checked="center.active"
                                       @change="toggleActive(center)" />
                                <span class="slider"></span>
                            </label>
                        </td>
                        <td>
                            <button class="btn-action" @click="editCenter(center)">✏️ Edit</button>
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

        <p v-else class="no-data">No training centers found.</p>
        <EditTrainingCenterModal v-if="editModalOpen"
                                 :center="editCenterData"
                                 @close="editModalOpen = false"
                                 @updated="fetchCenters" />
    </div>
</template>

<script>import CreateTrainingCenterModal from "@/components/Modals/CreateTrainingCenterModal.vue";
    import EditTrainingCenterModal from "@/components/Modals/EditTrainingCenterModal.vue";
    import apiClient from "@/axios";

    export default {
        components: { CreateTrainingCenterModal, EditTrainingCenterModal },
        data() {
            return {
                isModalOpen: false,
                editModalOpen: false,
                editCenterData: null,
                centers: [],
                currentPage: 1,
                pageSize: 10,
                totalItems: 0,
                filters: {
                    name: "",
                    zip: ""
                }
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.totalItems / this.pageSize);
            }
        },
        mounted() {
            this.fetchCenters();
        },
        methods: {
            async fetchCenters() {
                const params = {
                    page: this.currentPage,
                    pageSize: this.pageSize
                };
                if (this.filters.name.trim()) params.name = this.filters.name.trim();
                if (this.filters.zip.trim()) params.zip = this.filters.zip.trim();

                const res = await apiClient.get("/TrainingCenter/paged", { params });
                this.centers = res.data?.data?.$values ?? res.data.data;
                this.totalItems = res.data?.total ?? 0;
            },
            applyFilters() {
                this.currentPage = 1;
                this.fetchCenters();
            },
            resetFilters() {
                this.filters.name = "";
                this.filters.zip = "";
                this.applyFilters();
            },
            async toggleActive(center) {
                try {
                    const updated = { ...center, active: !center.active };
                    await apiClient.put(`/TrainingCenter/updateActive/${center.siteSysId}`, updated);
                    center.active = updated.active;
                } catch (err) {
                    console.error("Toggle failed", err);
                    alert("❌ Failed to update status.");
                }
            },
            changePage(page) {
                if (page >= 1 && page <= this.totalPages) {
                    this.currentPage = page;
                    this.fetchCenters();
                }
            },
            editCenter(center) {
                this.editCenterData = center;
                this.editModalOpen = true;
            }
        }
    };</script>


<style scoped>
    .system-page {
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
            font-size: 15px;
        }

        .modern-table th {
            background-color: #f8f9fa;
            font-weight: 600;
            white-space: nowrap;
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
