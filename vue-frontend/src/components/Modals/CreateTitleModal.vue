<template>
    <div class="modal-overlay">
        <div class="modal fade-in">

            <!-- Close -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- Header -->
            <header class="modal-header">
                <div>
                    <h2>Create New Training Title</h2>
                    <p class="modal-subtitle">
                        Define course metadata, credits, category & certificate details.
                    </p>
                </div>
                <span class="modal-badge">Admin • Title Creator</span>
            </header>

            <form @submit.prevent="submitTitle">

                <!-- BASIC INFORMATION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Basic Information</h3>
                        <p>Enter the main details for this title.</p>
                    </div>

                    <div class="form-group">
                        <label>Course Title <span class="required">*</span></label>
                        <input v-model="form.courseTitle" placeholder="Enter course title" required />
                    </div>
                </section>

                <!-- DESCRIPTION -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Description</h3>
                        <p>Enter detailed content for this course title.</p>
                    </div>

                    <quill-editor v-model:content="form.description"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box resizable" />
                </section>

                <!-- CATEGORY & CREDITS -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Category & Credits</h3>
                        <p>Choose category and credit types applicable.</p>
                    </div>

                    <div class="form-group">
                        <label>Category</label>
                        <select v-model="form.category">
                            <option value="">-- Select Category --</option>
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
                        <h3>Certificate Details</h3>
                        <p>Add certificate description and notes.</p>
                    </div>

                    <label>Certificate Description</label>
                    <quill-editor v-model:content="form.certDescription"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box resizable" />

                    <div class="form-group">
                        <label>Certificate Notes</label>
                        <textarea v-model="form.miscCertDesc"
                                  class="resizable-textarea"
                                  placeholder="Enter special notes"></textarea>
                    </div>
                </section>

                <!-- ONLINE TRAINING -->
                <section class="section-card">
                    <div class="section-header">
                        <h3>Delivery Format</h3>
                        <p>Specify whether this title is online.</p>
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
                               type="text"
                               placeholder="Enter webcast or training URL"
                               :required="form.isOnlineTraining === 'true'"
                               :class="{ 'required-border': form.isOnlineTraining === 'true' && !form.videoUrl }" />
                    </div>

                    <div class="form-group" v-if="form.isOnlineTraining === 'true'">
                        <label>Mark as New Until (optional)</label>
                        <input type="date" v-model="form.markAsNewUntil" />
                    </div>
                </section>

                <!-- BUTTONS -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                    <button type="submit" class="btn-primary">Create</button>
                </div>

            </form>
        </div>
    </div>
</template>
<script>import apiClient from '@/axios';
    import { QuillEditor } from '@vueup/vue-quill';

    export default {
        components: { QuillEditor },
        emits: ['close', 'created'],
        data() {
            return {
                form: {
                    courseTitle: '',
                    description: '',
                    category: '',
                    cnecredits: false,
                    oasascredits: false,
                    certDescription: '',
                    miscCertDesc: '',
                    videoUrl: '',
                    isOnlineTraining: false,
                    markAsNewUntil: null
                },
                categories: []
            };
        },
        async mounted() {
            const res = await apiClient.get('/Lookup/categories');
            this.categories = res.data?.$values || [];
        },
        methods: {
            async submitTitle() {
                try {
                    if (this.form.isOnlineTraining === 'true' && !this.form.videoUrl.trim()) {
                        alert('Please provide a WebCast or Online Training URL.');
                        return;
                    }
                    const payload = {
                        courseTitle: this.form.courseTitle,
                        description: this.form.description,
                        category: parseInt(this.form.category),
                        cnecredits: this.form.cnecredits,
                        oasascredits: this.form.oasascredits,
                        certDescription: this.form.certDescription,
                        miscCertDesc: this.form.miscCertDesc,
                        videoUrl: this.form.videoUrl,
                        isOnlineTraining: this.form.isOnlineTraining === 'true',
                        markAsNewUntil: this.form.markAsNewUntil

                    };

                    await apiClient.post('/TrainingTitle/create', payload);
                    alert('Training title created successfully!');
                    this.$emit('created');
                    this.$emit('close');
                } catch (err) {
                    console.error('Error creating title', err);
                    alert('Failed to create title.');
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
        max-width: 100%;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15,23,42,0.25), 0 0 0 1px rgba(148,163,184,0.35);
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
        font-size: 20px;
        cursor: pointer;
    }

    /* PURPLE HEADER */
    .modal-header {
        background: #43285D; /* Final purple */
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

    .modal-subtitle {
        font-size: 14px;
        opacity: 0.9;
    }

    .modal-badge {
        background: rgba(255,255,255,0.15);
        padding: 8px 18px;
        border-radius: 999px;
        font-weight: 600;
        font-size: 12px;
        color: white;
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

    /* FORM */
    input, select, textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-size: 14px;
    }

        textarea.resizable-textarea {
            min-height: 120px;
            resize: vertical;
        }

    /* QUILL EDITOR — RESIZABLE */
    .quill-box.resizable {
        min-height: 200px;
        resize: vertical;
        overflow: auto;
        border-radius: 12px;
    }

    /* RADIO BUTTONS */
    .radio-group {
        display: flex;
        gap: 20px;
        margin-top: 6px;
    }

    .radio-item {
        display: flex;
        align-items: center;
        gap: 6px;
        padding: 8px 16px;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background: #fafafa;
        cursor: pointer;
    }

        .radio-item input {
            width: 16px;
            height: 16px;
        }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary {
        background: #43285D; /* Apply purple here */
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
    }

        .btn-primary:hover {
            background: #361F4A;
        }

    .btn-secondary {
        background: #e5e7eb;
        padding: 10px 22px;
        border-radius: 999px;
    }
    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary,
    .btn-secondary {
        transition: all 0.25s ease;
        transform: translateY(0);
    }

    /* PRIMARY BUTTON */
    .btn-primary {
        background: #43285D;
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(67, 40, 93, 0.22);
    }

        .btn-primary:hover {
            background: #361F4A;
            transform: translateY(-3px);
            box-shadow: 0 8px 18px rgba(67, 40, 93, 0.32);
        }

    /* SECONDARY BUTTON */
    .btn-secondary {
        background: #e5e7eb;
        color: #333;
        padding: 10px 22px;
        border-radius: 999px;
        font-weight: 500;
        box-shadow: 0 2px 6px rgba(0,0,0,0.10);
    }

        .btn-secondary:hover {
            background: #d4d4d4;
            transform: translateY(-3px);
            box-shadow: 0 6px 14px rgba(0,0,0,0.15);
        }
</style>