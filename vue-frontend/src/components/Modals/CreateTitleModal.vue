<template>
    <div class="modal-overlay">
        <div class="modal fade-in">
            <!-- ✖ Close Button -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- Header -->
            <div class="modal-header">
                <h2>Create New Training Title</h2>
            </div>

            <form @submit.prevent="submitTitle" class="modal-body">
                <!-- Course Title -->
                <div class="form-group">
                    <label>Course Title <span class="required">*</span></label>
                    <input v-model="form.courseTitle" placeholder="Enter course title" required />
                </div>

                <!-- Description -->
                <div class="form-group">
                    <label>Description</label>
                    <quill-editor v-model:content="form.description"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />
                </div>

                <!-- Category -->
                <div class="form-group">
                    <label>Category</label>
                    <select v-model="form.category">
                        <option value="">-- Select Category --</option>
                        <option v-for="cat in categories"
                                :key="cat.code"
                                :value="cat.code">
                            {{ cat.value }}
                        </option>
                    </select>
                </div>

                <!-- Credits -->
                <div class="checkbox-row">
                    <label><input type="checkbox" v-model="form.cnecredits" /> CNE Hours</label>
                    <label><input type="checkbox" v-model="form.oasascredits" /> OASAS Hours</label>
                </div>

                <!-- Certificate Description -->
                <div class="form-group">
                    <label>Certificate Description</label>
                    <quill-editor v-model:content="form.certDescription"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box" />
                </div>

                <!-- Notes -->
                <div class="form-group">
                    <label>Certificate Notes</label>
                    <textarea v-model="form.miscCertDesc"
                              placeholder="Enter any special notes or remarks"></textarea>
                </div>

                <!-- Online Training -->
                <div class="form-group">
                    <label>Is it an Online Training?</label>
                    <div class="radio-row">
                        <label><input type="radio" value="true" v-model="form.isOnlineTraining" /> Yes</label>
                        <label><input type="radio" value="false" v-model="form.isOnlineTraining" /> No</label>
                    </div>
                </div>

                <!-- URL -->
                <div class="form-group">
                    <label>
                        WebCast or Online Training URL
                        <span v-if="form.isOnlineTraining === 'true'" class="required">*</span>
                    </label>
                    <input v-model="form.videoUrl"
                           type="text"
                           :required="form.isOnlineTraining === 'true'"
                           :class="{ 'required-border': form.isOnlineTraining === 'true' && !form.videoUrl }"
                           placeholder="Enter the webcast or training URL" />
                </div>

                <!-- Buttons -->
                <div class="button-group">
                    <button type="submit" class="btn-primary">Create</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
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
                    isOnlineTraining: false
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
                        isOnlineTraining: this.form.isOnlineTraining === 'true'
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
    /* ========== Overlay ========== */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.55);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    /* ========== Modal Shell ========== */
    .modal {
        position: relative;
        background-color: #fff;
        border-radius: 12px;
        width: 720px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 12px 32px rgba(0, 0, 0, 0.25);
        animation: fadeIn 0.25s ease-out;
        font-family: "Segoe UI", Roboto, sans-serif;
    }

    .fade-in {
        animation: fadeIn 0.3s ease-out;
    }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: scale(0.97);
        }

        to {
            opacity: 1;
            transform: scale(1);
        }
    }

    /* ========== Header ========== */
    .modal-header {
        background: #3D2B69;
        color: white;
        padding: 16px 24px;
        border-top-left-radius: 12px;
        border-top-right-radius: 12px;
        text-align: center;
    }

        .modal-header h2 {
            margin: 0;
            font-size: 22px;
            font-weight: 600;
        }

    /* ========== Body ========== */
    .modal-body {
        padding: 24px 28px;
    }

    /* ========== Form Fields ========== */
    .form-group {
        margin-bottom: 18px;
    }

        .form-group label {
            font-weight: 600;
            display: block;
            margin-bottom: 6px;
            color: #333;
        }

    input,
    select,
    textarea {
        width: 100%;
        padding: 10px 12px;
        border: 1px solid #ccc;
        border-radius: 6px;
        font-size: 15px;
        transition: border-color 0.2s ease;
    }

        input:focus,
        select:focus,
        textarea:focus {
            border-color: #1976d2;
            outline: none;
            box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.15);
        }

    textarea {
        resize: vertical;
        min-height: 90px;
    }

    .quill-box {
        border: 1px solid #ccc;
        border-radius: 6px;
        min-height: 130px;
    }

    /* ========== Checkboxes & Radio ========== */
    .checkbox-row,
    .radio-row {
        display: flex;
        gap: 20px;
        flex-wrap: wrap;
    }

        .radio-row label,
        .checkbox-row label {
            font-weight: 500;
            cursor: pointer;
            user-select: none;
        }

    /* ========== Buttons ========== */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 14px;
        margin-top: 28px;
    }

    .btn-primary {
        background: #1976d2;
        color: white;
        border: none;
        padding: 10px 22px;
        font-size: 15px;
        border-radius: 6px;
        cursor: pointer;
        font-weight: 600;
        transition: background 0.2s ease;
    }

        .btn-primary:hover {
            background: #1565c0;
        }

    .btn-secondary {
        background: #e0e0e0;
        color: #333;
        border: none;
        padding: 10px 22px;
        font-size: 15px;
        border-radius: 6px;
        cursor: pointer;
        transition: background 0.2s ease;
    }

        .btn-secondary:hover {
            background: #d5d5d5;
        }

    /* ========== Close Button ========== */
    .close-btn {
        position: absolute;
        top: 10px;
        right: 12px;
        background: rgba(255, 255, 255, 0.8);
        color: #333;
        border: none;
        border-radius: 50%;
        width: 32px;
        height: 32px;
        font-size: 20px;
        font-weight: bold;
        line-height: 1;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s ease;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
    }

        .close-btn:hover {
            background: #ffebee;
            color: #c62828;
            transform: scale(1.05);
        }

    /* ========== Misc ========== */
    .required {
        color: red;
    }

    .required-border {
        border-color: red !important;
    }
</style>