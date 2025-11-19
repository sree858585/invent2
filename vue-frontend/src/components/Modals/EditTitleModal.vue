<template>
    <div class="modal-overlay">
        <div class="modal fade-in">

            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- PURPLE HEADER (MATCHES CREATE & SCHEDULE) -->
            <header class="modal-header">
                <div>
                    <h2>Edit Training Title</h2>
                    <p class="modal-subtitle">Modify course metadata, credits, category & certificate details.</p>
                </div>
                <span class="modal-badge">Admin • Title Editor</span>
            </header>

            <!-- Body -->
            <form @submit.prevent="updateTitle">

                <!-- BASIC INFO -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Basic Information</h3>
                        <p>Edit title name and primary metadata.</p>
                    </div>

                    <div class="form-group">
                        <label>Course Title *</label>
                        <input v-model="form.courseTitle" required />
                    </div>
                </section>

                <!-- DESCRIPTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Description</h3>
                        <p>Update detailed descriptive content.</p>
                    </div>

                    <quill-editor v-model:content="form.description"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />
                </section>

                <!-- CATEGORY / CREDITS -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Category & Credits</h3>
                        <p>Manage title category and related credit types.</p>
                    </div>

                    <div class="form-group">
                        <label>Category</label>
                        <select v-model="form.category">
                            <option value="">-- Select --</option>
                            <option v-for="cat in categories" :key="cat.code" :value="cat.code">
                                {{ cat.value }}
                            </option>
                        </select>
                    </div>

                    <div class="checkbox-row">
                        <label><input type="checkbox" v-model="form.cnecredits" /> CNE Hours</label>
                        <label><input type="checkbox" v-model="form.oasascredits" /> OASAS Hours</label>
                    </div>
                </section>

                <!-- CERTIFICATE -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Certificate</h3>
                        <p>Edit certificate description & notes.</p>
                    </div>

                    <quill-editor v-model:content="form.certDescription"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />

                    <div class="form-group">
                        <label>Certificate Notes</label>
                        <textarea v-model="form.miscCertDesc" class="auto-expand"></textarea>
                    </div>
                </section>

                <!-- ONLINE SECTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Delivery Format</h3>
                        <p>Specify online training details & URLs.</p>
                    </div>

                    <div class="form-group">
                        <label>Is it an Online Training?</label>
                        <div class="radio-group">
                            <label class="radio-item">
                                <input type="radio" value="true" v-model="form.isOnlineTraining" />
                                <span>Yes</span>
                            </label>

                            <label class="radio-item">
                                <input type="radio" value="false" v-model="form.isOnlineTraining" />
                                <span>No</span>
                            </label>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>
                            WebCast / Online Training URL
                            <span v-if="form.isOnlineTraining === 'true'" class="required">*</span>
                        </label>
                        <input v-model="form.videoUrl"
                               :required="form.isOnlineTraining === 'true'"
                               :class="{ 'required-border': form.isOnlineTraining === 'true' && !form.videoUrl }" />
                    </div>

                    <div class="form-group" v-if="form.isOnlineTraining === 'true'">
                        <label>Mark as New Until (optional)</label>
                        <input type="date" v-model="form.markAsNewUntil" class="date-input" />
                    </div>
                </section>

                <!-- BUTTONS -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn-primary">Update</button>
                </div>

            </form>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios';
    import { QuillEditor } from '@vueup/vue-quill';

    export default {
        components: { QuillEditor },
        props: ['title'],
        emits: ['close', 'updated'],
        data() {
            return {
                form: {
                    subjectSysId: null,
                    courseTitle: '',
                    description: '',
                    category: '',
                    cnecredits: false,
                    oasascredits: false,
                    certDescription: '',
                    miscCertDesc: '',
                    videoUrl: '',
                    isOnlineTraining: 'false',
                    markAsNewUntil: null
                },
                categories: []
            };
        },
        async mounted() {
            const res = await apiClient.get('/Lookup/categories');
            this.categories = res.data?.$values || [];

            const t = await apiClient.get(`/TrainingTitle/${this.title.subjectSysId}`);
            const data = t.data;

            this.form = {
                subjectSysId: data.subjectSysId,
                courseTitle: data.courseTitle ?? '',
                description: data.description ?? '',
                category: data.category ?? '',
                cnecredits: data.cnecredits ?? false,
                oasascredits: data.oasascredits ?? false,
                certDescription: data.certDescription ?? '',
                miscCertDesc: data.miscCertDesc ?? '',
                videoUrl: data.videoUrl ?? '',
                isOnlineTraining: data.isOnlineTraining ? 'true' : 'false',
                markAsNewUntil: data.markAsNewUntil
                    ? data.markAsNewUntil.substring(0, 10)
                    : null
            };
        },
        methods: {
            async updateTitle() {
                try {
                    if (this.form.isOnlineTraining === 'true' && !this.form.videoUrl.trim()) {
                        alert('Please provide a WebCast or Online Training URL.');
                        return;
                    }

                    const payload = {
                        ...this.form,
                        isOnlineTraining: this.form.isOnlineTraining === 'true',
                        markAsNewUntil: this.form.markAsNewUntil

                    };

                    await apiClient.put(`/TrainingTitle/update/${this.form.subjectSysId}`, payload);
                    alert('Training title updated successfully!');
                    this.$emit('updated');
                    this.$emit('close');
                } catch (err) {
                    console.error('Error updating title', err);
                    alert('Failed to update training title.');
                }
            }
        }
    };</script>

<style scoped>
    /* OVERLAY */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(circle at top, rgba(15,23,42,0.3), rgba(15,23,42,0.75));
        backdrop-filter: blur(6px);
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 24px;
        z-index: 999;
    }

    /* MODAL */
    .modal {
        position: relative;
        background: #ffffff;
        border-radius: 24px;
        width: 1100px;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15,23,42,0.25), 0 0 0 1px rgba(148,163,184,0.35);
        font-family: system-ui, "Segoe UI", sans-serif;
    }

    /* CLOSE BUTTON */
    .close-btn {
        position: absolute;
        top: 14px;
        right: 16px;
        background: rgba(255,255,255,0.9);
        border: none;
        border-radius: 50%;
        width: 34px;
        height: 34px;
        cursor: pointer;
        font-size: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

        .close-btn:hover {
            background: #ffebee;
            color: #c62828;
        }

    /* HEADER */
    .modal-header {
        background: #43285D;
        color: white;
        margin: -32px -32px 24px -32px;
        padding: 28px 40px;
        border-top-left-radius: 24px;
        border-top-right-radius: 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .modal-header h2 {
            font-size: 28px;
            font-weight: 700;
            margin: 0;
        }

    .modal-badge {
        background: rgba(255,255,255,0.18);
        color: white;
        padding: 8px 20px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 600;
    }

    /* SECTION CARD */
    .section-card {
        background: white;
        border-radius: 18px;
        padding: 20px;
        border: 1px solid #e5e7eb;
        margin-bottom: 20px;
        box-shadow: 0 12px 30px rgba(15,23,42,0.08);
    }

    .section-header h3 {
        font-size: 17px;
        font-weight: 600;
        margin: 0;
    }

    .section-header p {
        font-size: 13px;
        color: #6b7280;
        margin: 4px 0 12px;
    }

    /* INPUTS */
    input, select, textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
    }

    textarea {
        resize: vertical;
        min-height: 110px;
    }

    /* QUILL EDITOR RESIZABLE */
    .quill-box {
        border: 1px solid #d1d5db;
        border-radius: 12px;
        min-height: 150px;
        resize: vertical;
        overflow: auto;
    }

    /* RADIO BUTTONS (MATCH CREATE TITLE) */
    .radio-group {
        display: flex;
        gap: 16px;
        margin-top: 8px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        border: 1px solid #d1d5db;
        border-radius: 999px;
        background: #f9fafb;
        cursor: pointer;
        transition: all 0.2s ease;
    }

        .radio-item:hover {
            background: #f1eef7;
            border-color: #43285D;
        }

        .radio-item input[type="radio"] {
            accent-color: #43285D;
            width: 16px;
            height: 16px;
        }

        .radio-item input:checked + span {
            color: #43285D;
            font-weight: 600;
        }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary {
        background: #43285D;
        padding: 10px 22px;
        color: white;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.25s ease;
    }

        .btn-primary:hover {
            background: #341F49;
        }

    .btn-secondary {
        background: #e5e7eb;
        padding: 10px 22px;
        border-radius: 999px;
        cursor: pointer;
        transition: background 0.25s ease;
    }

        .btn-secondary:hover {
            background: #d5d5d5;
        }

    /* REQUIRED */
    .required {
        color: red;
    }

    .required-border {
        border-color: red !important;
    }
    .radio-group {
        display: flex;
        gap: 16px;
        margin-top: 8px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 18px;
        border: 1px solid #d1d5db;
        border-radius: 999px;
        background: #f9fafb;
        cursor: pointer;
        transition: all 0.2s ease;
    }

        .radio-item:hover {
            background: #f1eef7;
            border-color: #43285D;
        }

        .radio-item input[type="radio"] {
            accent-color: #43285D;
            width: 16px;
            height: 16px;
        }

        .radio-item input:checked + span {
            color: #43285D;
            font-weight: 600;
        }
</style>