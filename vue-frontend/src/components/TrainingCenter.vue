<template>
    <div class="system-page">
        <div class="header">
            <h2>🏢 Training Centers</h2>
        </div>
        <div class="cta-row">
            <button class="btn-primary btn-cta" @click="isModalOpen = true">
                ➕ Add New Center
            </button>
        </div>

        <div class="filter-panel">
            <div class="filter-group">
                <!-- Name (auto search on input) -->
                <input v-model="filters.name"
                       placeholder="Search by Name..."
                       @input="applyFilters" />

                <!-- Region multi-select dropdown with checkboxes -->
                <div class="region-multiselect" @click.stop>
                    <button class="region-trigger" @click="regionOpen = !regionOpen">
                        <span v-if="!filters.regions.length">Filter by Region</span>
                        <span v-else>{{ selectedRegionLabels.join(', ') }}</span>
                        <svg viewBox="0 0 20 20" class="chev"><path d="M5 7l5 6 5-6" /></svg>
                    </button>

                    <div v-if="regionOpen" class="region-menu">
                        <div class="region-search">
                            <input v-model="regionQuery" placeholder="Search regions..." />
                        </div>
                        <div class="region-list">
                            <label v-for="r in filteredRegions"
                                   :key="r.code"
                                   class="region-item">
                                <input type="checkbox"
                                       :value="r.code"
                                       v-model="filters.regions"
                                       @change="onRegionsChanged" />
                                <span>{{ r.name }}</span>
                            </label>
                        </div>
                        <div class="region-actions">
                            <button type="button" class="link" @click="clearRegions">Clear</button>
                            <button type="button" class="link" @click="regionOpen = false">Close</button>
                        </div>
                    </div>
                </div>
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
                        <th>Active</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="center in centers" :key="center.siteSysId">
                        <td class="truncate-cell" :title="center.siteName">{{ center.siteName }}</td>
                        <td>
                            {{ center.active ? 'Yes' : 'No' }}
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

                // filters
                filters: {
                    name: "",
                    regions: [] // array of region codes (numbers)
                },

                // regions dropdown
                regions: [],
                regionOpen: false,
                regionQuery: "",
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.totalItems / this.pageSize);
            },
            filteredRegions() {
                const q = this.regionQuery.trim().toLowerCase();
                if (!q) return this.regions;
                return this.regions.filter(r => r.name.toLowerCase().includes(q));
            },
            selectedRegionLabels() {
                const map = new Map(this.regions.map(r => [r.code, r.name]));
                return this.filters.regions.map(code => map.get(code)).filter(Boolean);
            }
        },
        mounted() {
            document.addEventListener("click", this.closeRegionIfClickOutside);
            this.bootstrap();
        },
        beforeUnmount() {
            document.removeEventListener("click", this.closeRegionIfClickOutside);
        },
        methods: {
            async bootstrap() {
                const [regionsRes] = await Promise.all([
                    apiClient.get("/TrainingCenter/regions"),
                ]);
                this.regions = regionsRes.data?.$values ?? regionsRes.data ?? [];
                await this.fetchCenters();
            },
            async fetchCenters() {
                const params = {
                    page: this.currentPage,
                    pageSize: this.pageSize,
                };
                if (this.filters.name.trim()) params.name = this.filters.name.trim();
                if (this.filters.regions.length) {
                    // send CSV of region codes: e.g. "1,3,7"
                    params.regions = this.filters.regions.join(",");
                }

                const res = await apiClient.get("/TrainingCenter/paged", { params });
                this.centers = res.data?.data?.$values ?? res.data.data ?? [];
                this.totalItems = res.data?.total ?? 0;
            },
            applyFilters() {
                this.currentPage = 1;
                this.fetchCenters();
            },
            onRegionsChanged() {
                this.applyFilters();
            },
            clearRegions() {
                this.filters.regions = [];
                this.applyFilters();
            },
            closeRegionIfClickOutside() {
                if (this.regionOpen) this.regionOpen = false;
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
    system-page {
        padding: 20px 40px;
        font-family: 'Segoe UI', sans-serif;
        color: #333;
    }

    .header {
        display: flex;
        justify-content: center; 
        align-items: center;
        margin-bottom: 16px;
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
    /* Region multi-select */
    .region-multiselect {
        position: relative;
    }

    .region-trigger {
        width: 100%;
        text-align: left;
        padding: 10px 14px;
        border: 1px solid #ccc;
        border-radius: 12px;
        background: #fff;
        display: flex;
        align-items: center;
        gap: 8px;
        justify-content: space-between;
        cursor: pointer;
    }

        .region-trigger .chev {
            width: 16px;
            height: 16px;
            fill: none;
            stroke: #666;
            stroke-width: 2;
        }

    .region-menu {
        position: absolute;
        z-index: 5;
        margin-top: 6px;
        background: #fff;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        width: 100%;
        max-height: 280px;
        overflow: hidden;
        box-shadow: 0 10px 30px rgba(0,0,0,.08);
    }

    .region-search {
        padding: 8px;
        border-bottom: 1px solid #eee;
    }

        .region-search input {
            width: 100%;
            padding: 8px 10px;
            border: 1px solid #ddd;
            border-radius: 8px;
        }

    .region-list {
        max-height: 210px;
        overflow-y: auto;
        padding: 6px 8px;
    }

    .region-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 6px 4px;
        cursor: pointer;
        border-radius: 6px;
    }

        .region-item:hover {
            background: #f8fafc;
        }

    .region-actions {
        display: flex;
        justify-content: space-between;
        padding: 8px 10px;
        border-top: 1px solid #eee;
        background: #fafafa;
    }

        .region-actions .link {
            background: none;
            border: none;
            color: #2563eb;
            cursor: pointer;
            padding: 4px 8px;
            border-radius: 6px;
        }

            .region-actions .link:hover {
                background: #eef2ff;
            }
    .header {
        display: flex;
        justify-content: center; /* center the heading */
        align-items: center;
        margin-bottom: 12px;
    }

        .header h2 {
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    /* Centered CTA row under the title */
    .cta-row {
        display: flex;
        justify-content: center;
        margin-bottom: 18px;
    }

    /* Base button */
    .btn-primary {
        background-color: #4caf50;
        color: white;
        border: none;
        padding: 12px 22px;
        font-size: 16px;
        border-radius: 10px;
        cursor: pointer;
        transition: background-color 0.25s ease, transform 0.05s ease;
    }

        .btn-primary:hover {
            background-color: #388e3c;
        }

        .btn-primary:active {
            transform: translateY(1px);
        }

    /* Bigger CTA look */
    .btn-cta {
        padding: 14px 28px;
        font-size: 18px;
        border-radius: 12px;
    }
</style>

