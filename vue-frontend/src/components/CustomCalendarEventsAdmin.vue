<template>
    <div class="page">
        <h1 class="title">Custom Calendar Events (Admin)</h1>

        <div class="card">
            <h2 class="section-title">
                {{ isEditMode ? "Edit Event" : "Create Event" }}
            </h2>

            <form class="form" @submit.prevent="submitForm">
                <div class="row">
                    <label>Title *</label>
                    <input v-model.trim="form.title" type="text" maxlength="200" required />
                </div>

                <div class="row">
                    <label>Short Description</label>
                    <input v-model.trim="form.shortDescription" type="text" maxlength="400" />
                </div>

                <div class="row">
                    <label>Long Description</label>
                    <textarea v-model.trim="form.longDescription" rows="4"></textarea>
                </div>

                <div class="grid2">
                    <div class="row">
                        <label>Start (local) *</label>
                        <input v-model="form.startLocal" type="datetime-local" required />
                    </div>

                    <div class="row">
                        <label>End (local)</label>
                        <input v-model="form.endLocal" type="datetime-local" :disabled="form.allDay" />
                    </div>
                </div>

                <div class="grid2">
                    <div class="row">
                        <label>Category</label>
                        <input v-model.trim="form.category"
                               type="text"
                               maxlength="50"
                               placeholder="Holiday / Maintenance / Meeting" />
                    </div>

                    <div class="row">
                        <label>URL</label>
                        <input v-model.trim="form.url" type="url" maxlength="500" placeholder="https://..." />
                    </div>
                </div>

                <div class="grid2">
                    <div class="row">
                        <label>Color (hex)</label>
                        <input v-model.trim="form.color" type="text" maxlength="10" placeholder="#6e528d" />
                    </div>

                    <div class="row">
                        <label>Status</label>
                        <select v-model="form.isActive">
                            <option :value="true">Active</option>
                            <option :value="false">Inactive</option>
                        </select>
                    </div>
                </div>

                <div class="row inline">
                    <input id="allday" type="checkbox" v-model="form.allDay" />
                    <label for="allday">All Day</label>
                </div>

                <div class="actions">
                    <button type="submit" :disabled="saving">
                        {{ saving ? "Saving..." : (isEditMode ? "Update Event" : "Create Event") }}
                    </button>

                    <button type="button" class="secondary" @click="resetForm" :disabled="saving">
                        {{ isEditMode ? "Cancel Edit" : "Reset" }}
                    </button>
                </div>

                <p v-if="error" class="error">{{ error }}</p>
                <p v-if="success" class="success">{{ success }}</p>
            </form>
        </div>

        <div class="card">
            <div class="header-row">
                <h2 class="section-title">Existing Events</h2>
                <button class="secondary" @click="loadEvents" :disabled="loading">
                    {{ loading ? "Loading..." : "Refresh" }}
                </button>
            </div>

            <div v-if="loading" class="muted">Loading events...</div>
            <div v-else-if="events.length === 0" class="muted">No custom events found.</div>

            <div v-else class="table">
                <div class="thead">
                    <div>Title</div>
                    <div>Start</div>
                    <div>End</div>
                    <div>Active</div>
                    <div></div>
                </div>

                <div class="trow" v-for="e in events" :key="e.customCalendarEventId">
                    <div class="cell">
                        <div class="strong">{{ e.title }}</div>
                        <div class="small muted">{{ e.shortDescription }}</div>
                    </div>
                    <div class="cell">{{ formatDate(e.startUtc) }}</div>
                    <div class="cell">{{ e.endUtc ? formatDate(e.endUtc) : "-" }}</div>
                    <div class="cell">{{ e.isActive ? "Yes" : "No" }}</div>

                    <div class="cell actions-cell">
                        <!-- ✅ EDIT button (before Delete) -->
                        <button class="secondary"
                                @click="editEvent(e)"
                                :disabled="saving || deletingId === e.customCalendarEventId">
                            Edit
                        </button>

                        <button class="danger"
                                @click="removeEvent(e.customCalendarEventId)"
                                :disabled="deletingId === e.customCalendarEventId || saving">
                            {{ deletingId === e.customCalendarEventId ? "Deleting..." : "Delete" }}
                        </button>
                    </div>
                </div>
            </div>

            <p class="muted tip">
                Tip: after creating/updating/deleting, go to the Calendar page and refresh (or we can auto-refetch).
            </p>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        name: "CustomCalendarEventsAdmin",
        data() {
            return {
                loading: false,
                saving: false,
                deletingId: null,
                events: [],
                error: "",
                success: "",

                // ✅ edit state
                editingId: null,

                form: {
                    title: "",
                    shortDescription: "",
                    longDescription: "",
                    startLocal: "",
                    endLocal: "",
                    allDay: false,
                    category: "",
                    url: "",
                    color: "",
                    isActive: true,
                },
            };
        },
        computed: {
            isEditMode() {
                return !!this.editingId;
            },
        },
        mounted() {
            this.loadEvents();
        },
        methods: {
            resetForm() {
                this.error = "";
                this.success = "";
                this.editingId = null;
                this.form = {
                    title: "",
                    shortDescription: "",
                    longDescription: "",
                    startLocal: "",
                    endLocal: "",
                    allDay: false,
                    category: "",
                    url: "",
                    color: "",
                    isActive: true,
                };
            },

            toUtcIso(localInput) {
                const d = new Date(localInput);
                if (isNaN(d.getTime())) return null;
                return d.toISOString();
            },

            // ✅ Convert UTC ISO -> datetime-local string "YYYY-MM-DDTHH:mm"
            toLocalDatetimeInput(utcIso) {
                if (!utcIso) return "";
                const d = new Date(utcIso);
                if (isNaN(d.getTime())) return "";
                const pad = (n) => String(n).padStart(2, "0");
                return (
                    d.getFullYear() +
                    "-" +
                    pad(d.getMonth() + 1) +
                    "-" +
                    pad(d.getDate()) +
                    "T" +
                    pad(d.getHours()) +
                    ":" +
                    pad(d.getMinutes())
                );
            },

            async loadEvents() {
                this.loading = true;
                this.error = "";
                try {
                    const res = await apiClient.get("/CustomCalendarEvents");
                    const data = Array.isArray(res.data)
                        ? res.data
                        : Array.isArray(res.data?.$values)
                            ? res.data.$values
                            : [];
                    this.events = data;
                } catch (err) {
                    this.error = err?.response?.data?.message || "Failed to load custom events.";
                    console.error(err?.response?.data || err);
                } finally {
                    this.loading = false;
                }
            },

            // ✅ Called by form submit (Create or Update)
            async submitForm() {
                if (this.isEditMode) return this.updateEvent();
                return this.createEvent();
            },

            async createEvent() {
                this.saving = true;
                this.error = "";
                this.success = "";

                try {
                    const startUtc = this.toUtcIso(this.form.startLocal);
                    if (!startUtc) throw new Error("Invalid start date/time");

                    const payload = {
                        title: this.form.title,
                        shortDescription: this.form.shortDescription || null,
                        longDescription: this.form.longDescription || null,
                        startUtc,
                        endUtc: this.form.allDay
                            ? null
                            : this.form.endLocal
                                ? this.toUtcIso(this.form.endLocal)
                                : null,
                        allDay: this.form.allDay,
                        category: this.form.category || null,
                        url: this.form.url || null,
                        color: this.form.color || null,
                        isActive: this.form.isActive,
                    };

                    await apiClient.post("/CustomCalendarEvents", payload);

                    this.success = "Event created successfully.";
                    this.resetForm();
                    await this.loadEvents();
                } catch (err) {
                    this.error =
                        err?.response?.data?.message || err?.message || "Failed to create event.";
                    console.error(err?.response?.data || err);
                } finally {
                    this.saving = false;
                }
            },

            // ✅ Prefill form and switch to edit mode
            editEvent(e) {
                if (!e?.customCalendarEventId) return;

                this.error = "";
                this.success = "";
                this.editingId = e.customCalendarEventId;

                this.form = {
                    title: e.title || "",
                    shortDescription: e.shortDescription || "",
                    longDescription: e.longDescription || "",
                    startLocal: this.toLocalDatetimeInput(e.startUtc),
                    endLocal: e.endUtc ? this.toLocalDatetimeInput(e.endUtc) : "",
                    allDay: !!e.allDay,
                    category: e.category || "",
                    url: e.url || "",
                    color: e.color || "",
                    isActive: e.isActive !== false, // default true
                };

                // scroll to top so admin sees the form
                window.scrollTo({ top: 0, behavior: "smooth" });
            },

            async updateEvent() {
                if (!this.editingId) return;

                this.saving = true;
                this.error = "";
                this.success = "";

                try {
                    const startUtc = this.toUtcIso(this.form.startLocal);
                    if (!startUtc) throw new Error("Invalid start date/time");

                    const payload = {
                        // include id if your controller expects it (safe either way)
                        customCalendarEventId: this.editingId,
                        title: this.form.title,
                        shortDescription: this.form.shortDescription || null,
                        longDescription: this.form.longDescription || null,
                        startUtc,
                        endUtc: this.form.allDay
                            ? null
                            : this.form.endLocal
                                ? this.toUtcIso(this.form.endLocal)
                                : null,
                        allDay: this.form.allDay,
                        category: this.form.category || null,
                        url: this.form.url || null,
                        color: this.form.color || null,
                        isActive: this.form.isActive,
                    };

                    // ✅ Update endpoint (typical REST)
                    await apiClient.put(`/CustomCalendarEvents/${this.editingId}`, payload);

                    this.success = "Event updated successfully.";
                    this.resetForm();
                    await this.loadEvents();
                } catch (err) {
                    this.error =
                        err?.response?.data?.message || err?.message || "Failed to update event.";
                    console.error(err?.response?.data || err);
                } finally {
                    this.saving = false;
                }
            },

            async removeEvent(id) {
                if (!id) return;
                if (!confirm("Delete this event?")) return;

                this.deletingId = id;
                this.error = "";
                this.success = "";

                try {
                    await apiClient.delete(`/CustomCalendarEvents/${id}`);

                    // if we deleted the one we were editing -> exit edit mode
                    if (this.editingId === id) this.resetForm();

                    this.success = "Event deleted.";
                    await this.loadEvents();
                } catch (err) {
                    this.error = err?.response?.data?.message || "Failed to delete event.";
                    console.error(err?.response?.data || err);
                } finally {
                    this.deletingId = null;
                }
            },

            formatDate(utc) {
                try {
                    const d = new Date(utc);
                    return d.toLocaleString();
                } catch {
                    return utc;
                }
            },
        },
    };</script>

<style scoped>
    .page {
        padding: 24px;
    }

    .title {
        font-size: 24px;
        font-weight: 700;
        color: #6e528d;
        margin-bottom: 16px;
    }

    .card {
        background: #fff;
        border-radius: 12px;
        padding: 18px;
        box-shadow: 0 4px 18px rgba(0,0,0,0.08);
        margin-bottom: 16px;
    }

    .section-title {
        font-size: 18px;
        font-weight: 700;
        margin: 0 0 12px;
        color: #333;
    }

    .form {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

    .row {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

        .row.inline {
            flex-direction: row;
            align-items: center;
            gap: 10px;
        }

    .grid2 {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px;
    }

    label {
        font-size: 13px;
        color: #444;
        font-weight: 600;
    }

    input, textarea, select {
        border: 1px solid #ddd;
        border-radius: 8px;
        padding: 10px;
        font-size: 14px;
        outline: none;
    }

        input:focus, textarea:focus, select:focus {
            border-color: #6e528d;
        }

    .actions {
        display: flex;
        gap: 10px;
        margin-top: 6px;
    }

    button {
        background: #6e528d;
        color: #fff;
        border: none;
        padding: 10px 14px;
        border-radius: 8px;
        cursor: pointer;
        font-weight: 600;
    }

        button.secondary {
            background: #eee;
            color: #333;
        }

        button.danger {
            background: #c0392b;
        }

        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

    .error {
        color: #c0392b;
        font-weight: 600;
    }

    .success {
        color: #2e7d32;
        font-weight: 600;
    }

    .muted {
        color: #777;
    }

    .small {
        font-size: 12px;
    }

    .strong {
        font-weight: 700;
    }

    .header-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 10px;
    }

    .table {
        display: grid;
        gap: 6px;
    }

    .thead, .trow {
        display: grid;
        grid-template-columns: 2fr 1fr 1fr 0.6fr 0.9fr;
        gap: 10px;
        align-items: center;
    }

    .thead {
        font-weight: 800;
        color: #444;
        padding: 8px 0;
        border-bottom: 1px solid #eee;
    }

    .trow {
        padding: 10px 0;
        border-bottom: 1px solid #f3f3f3;
    }

    .cell {
        overflow: hidden;
    }

    .actions-cell {
        display: flex;
        justify-content: flex-end;
        gap: 8px;
    }

    .tip {
        margin-top: 10px;
    }

    @media (max-width: 900px) {
        .grid2 {
            grid-template-columns: 1fr;
        }

        .thead, .trow {
            grid-template-columns: 1fr;
        }

        .actions-cell {
            justify-content: flex-start;
        }
    }
</style>