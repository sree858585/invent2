<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Edit Instructor</h2>
            <form @submit.prevent="submitEdit">
                <div class="form-grid">
                    <div class="form-group">
                        <label>Name *</label>
                        <input v-model="form.name" required />
                    </div>

                    <div class="form-group">
                        <label>Training Center *</label>
                        <select v-model="form.siteSysId" required>
                            <option value="">-- Select --</option>
                            <option v-for="site in sites" :key="site.siteSysId" :value="site.siteSysId">
                                {{ site.siteName }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Email</label>
                        <input v-model="form.email" type="email" />
                    </div>

                    <div class="form-group">
                        <label>Phone</label>
                        <input v-model="form.phone" />
                    </div>

                    <div class="form-group">
                        <label>Cell Phone</label>
                        <input v-model="form.cellPhone" />
                    </div>

                    <div class="form-group">
                        <label>Instructor Notes</label>
                        <textarea v-model="form.notes"></textarea>
                    </div>

                    <div class="form-group checkbox-group">
                        <label>
                            <input type="checkbox" v-model="form.active" />
                            Active
                        </label>
                    </div>
                </div>

                <div class="button-group">
                    <button type="submit" class="btn-primary">Update</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        props: ["instructor"],
        emits: ["close", "updated"],
        data() {
            return {
                form: { ...this.instructor,
                    notes: this.instructor.insNotes || ""
},
                sites: []
            };
        },
        async mounted() {
              console.log("Instructor prop:", this.instructor);
            const res = await apiClient.get("/Lookup/sites");
            this.sites = res.data?.$values ?? [];
        },
        methods: {
            async submitEdit() {
  try {
    const payload = {
      ...this.form,
      insNotes: this.form.notes
    };
    await apiClient.put(`/InstructorManagement/update/${this.instructor.instructorSysId}`, payload);
    alert("Instructor updated successfully!");
    this.$emit("updated");
    this.$emit("close");
  } catch (err) {
    console.error("Update failed:", err);
    alert("Error updating instructor.");
  }
}
        }
    };</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal {
        background-color: white;
        padding: 36px;
        border-radius: 16px;
        width: 640px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 16px 48px rgba(0, 0, 0, 0.2);
        font-family: "Segoe UI", sans-serif;
        animation: fadeIn 0.3s ease;
    }

        .modal h2 {
            font-size: 26px;
            font-weight: 600;
            margin-bottom: 24px;
            text-align: center;
            color: #2c2c2c;
        }

    .form-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 20px;
    }

    .form-group {
        display: flex;
        flex-direction: column;
        font-size: 14px;
    }

        .form-group label {
            margin-bottom: 6px;
            font-weight: 600;
            color: #444;
        }

    input,
    select,
    textarea {
        padding: 12px;
        font-size: 15px;
        border: 1px solid #ccc;
        border-radius: 10px;
        background-color: #f9f9f9;
        transition: all 0.3s ease;
    }

        input:focus,
        select:focus,
        textarea:focus {
            outline: none;
            border-color: #3f51b5;
            background-color: #fff;
            box-shadow: 0 0 0 3px rgba(63, 81, 181, 0.1);
        }

    textarea {
        resize: vertical;
        min-height: 80px;
    }

    .checkbox-group {
        margin-top: 8px;
        display: flex;
        align-items: center;
    }

        .checkbox-group input[type="checkbox"] {
            width: 18px;
            height: 18px;
            margin-right: 10px;
            accent-color: #3f51b5;
            cursor: pointer;
        }

        .checkbox-group label {
            font-weight: 500;
            cursor: pointer;
            user-select: none;
        }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 14px;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 12px 24px;
        border: none;
        font-size: 15px;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(63, 81, 181, 0.2);
        transition: background-color 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-primary:hover {
            background-color: #2f3e94;
            box-shadow: 0 6px 16px rgba(63, 81, 181, 0.3);
        }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 12px 24px;
        border: none;
        border-radius: 8px;
        font-size: 15px;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-secondary:hover {
            background-color: #d0d0d0;
        }

    /* Animation */
    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    /* Responsive tweaks */
    @media (max-width: 768px) {
        .modal {
            width: 95%;
            padding: 24px;
        }

        .button-group {
            flex-direction: column;
            align-items: stretch;
        }

        .btn-primary,
        .btn-secondary {
            width: 100%;
        }
    }
</style>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal {
        background-color: white;
        padding: 36px;
        border-radius: 16px;
        width: 640px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 16px 48px rgba(0, 0, 0, 0.2);
        font-family: "Segoe UI", sans-serif;
        animation: fadeIn 0.3s ease;
    }

        .modal h2 {
            font-size: 26px;
            font-weight: 600;
            margin-bottom: 24px;
            text-align: center;
            color: #2c2c2c;
        }

    .form-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 20px;
    }

    .form-group {
        display: flex;
        flex-direction: column;
        font-size: 14px;
    }

        .form-group label {
            margin-bottom: 6px;
            font-weight: 600;
            color: #444;
        }

    input,
    select,
    textarea {
        padding: 12px;
        font-size: 15px;
        border: 1px solid #ccc;
        border-radius: 10px;
        background-color: #f9f9f9;
        transition: all 0.3s ease;
    }

        input:focus,
        select:focus,
        textarea:focus {
            outline: none;
            border-color: #3f51b5;
            background-color: #fff;
            box-shadow: 0 0 0 3px rgba(63, 81, 181, 0.1);
        }

    textarea {
        resize: vertical;
        min-height: 80px;
    }

    .checkbox-group {
        margin-top: 8px;
        display: flex;
        align-items: center;
    }

        .checkbox-group input[type="checkbox"] {
            width: 18px;
            height: 18px;
            margin-right: 10px;
            accent-color: #3f51b5;
            cursor: pointer;
        }

        .checkbox-group label {
            font-weight: 500;
            cursor: pointer;
            user-select: none;
        }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 14px;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 12px 24px;
        border: none;
        font-size: 15px;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(63, 81, 181, 0.2);
        transition: background-color 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-primary:hover {
            background-color: #2f3e94;
            box-shadow: 0 6px 16px rgba(63, 81, 181, 0.3);
        }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 12px 24px;
        border: none;
        border-radius: 8px;
        font-size: 15px;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-secondary:hover {
            background-color: #d0d0d0;
        }

    /* Animation */
    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    /* Responsive tweaks */
    @media (max-width: 768px) {
        .modal {
            width: 95%;
            padding: 24px;
        }

        .button-group {
            flex-direction: column;
            align-items: stretch;
        }

        .btn-primary,
        .btn-secondary {
            width: 100%;
        }
    }
</style>

