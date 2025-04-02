<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Edit Training Title</h2>
            <form @submit.prevent="updateTitle">
                <div class="form-group">
                    <label>Course Title *</label>
                    <input v-model="form.courseTitle" required />
                </div>

                <div class="form-group">
                    <label>Description</label>
                    <textarea v-model="form.description"></textarea>
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
                    <label><input type="checkbox" v-model="form.active" /> Active</label>
                    <label><input type="checkbox" v-model="form.ai" /> AIDS Institute</label>
                    <label><input type="checkbox" v-model="form.cnecredits" /> CNE hours</label>
                    <label><input type="checkbox" v-model="form.oasascredits" /> OASAS hours</label>
                </div>

                <div class="row-group">
                    <div class="form-group small">
                        <label>Base Hours</label>
                        <input v-model="form.creditHrs" type="text" />
                    </div>
                    <div class="form-group small">
                        <label>3rd Party Course ID</label>
                        <input v-model="form.a3rdPartyCrseId" type="text" />
                    </div>
                </div>

                <div class="form-group">
                    <label>Certificate Description</label>
                    <textarea v-model="form.certDescription"></textarea>
                </div>

                <div class="form-group">
                    <label>Certificate Notes</label>
                    <textarea v-model="form.miscCertDesc"></textarea>
                </div>

                <div class="form-group">
                    <label>WebCast or Online Training URL</label>
                    <input v-model="form.videoUrl" type="text" />
                </div>

                <div class="button-group">
                    <button type="submit" class="btn-primary">Update</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios';

    export default {
        props: ['title'],
        emits: ['close', 'updated'],
        data() {
            return {
                form: {
                    subjectSysId: null,
                    courseTitle: '',
                    description: '',
                    category: '',
                    active: true,
                    ai: false,
                    cnecredits: false,
                    oasascredits: false,
                    creditHrs: '',
                    a3rdPartyCrseId: '',
                    certDescription: '',
                    miscCertDesc: '',
                    videoUrl: '',
                    is3rdParty: false
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
                active: data.active ?? false,
                ai: data.ai ?? false,
                cnecredits: data.cnecredits ?? false,
                oasascredits: data.oasascredits ?? false,
                creditHrs: data.creditHrs ?? '',
                a3rdPartyCrseId: data.a3rdPartyCrseId ?? '',
                certDescription: data.certDescription ?? '',
                miscCertDesc: data.miscCertDesc ?? '',
                videoUrl: data.videoUrl ?? '',
                is3rdParty: data.is3rdParty ?? false
            };
        },
        methods: {
            async updateTitle() {
                try {
                    await apiClient.put(`/TrainingTitle/update/${this.form.subjectSysId}`, this.form);
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
    /* styling same as CreateTitleModal.vue */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal {
        background-color: white;
        padding: 32px;
        border-radius: 12px;
        width: 700px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
    }

        .modal h2 {
            margin-bottom: 20px;
            font-size: 24px;
            font-weight: 600;
            text-align: center;
            color: #333;
        }

    .form-group {
        margin-bottom: 16px;
    }

        .form-group label {
            display: block;
            font-weight: 600;
            margin-bottom: 6px;
        }

    input,
    select,
    textarea {
        width: 100%;
        padding: 10px;
        border-radius: 6px;
        border: 1px solid #ccc;
        font-size: 14px;
    }

    textarea {
        resize: vertical;
        min-height: 80px;
    }

    .checkbox-row {
        display: flex;
        flex-wrap: wrap;
        gap: 16px;
        margin-bottom: 16px;
    }

    .row-group {
        display: flex;
        gap: 16px;
        margin-bottom: 16px;
    }

        .row-group .form-group.small {
            flex: 1;
        }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 24px;
    }

    .btn-primary {
        background-color: #4caf50;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
    }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 10px 20px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
    }
</style>
