<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Login to Your Account</h3>
            <form @submit.prevent="handleLogin">
                <div class="form-group">
                    <label for="email">Email Address</label>
                    <input type="email" v-model="email" placeholder="Enter your email" required />
                </div>
                <div class="form-group">
                    <label for="password">Password</label>
                    <input type="password" v-model="password" placeholder="Enter your password" required />
                </div>
                <button type="submit" class="btn-primary">Login</button>
            </form>
            <p>
                <a href="#" @click.prevent="$emit('show-register')">Register if you don't have an account.</a>
            </p>
            <button class="close-btn" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";

    export default {
        data() {
            return {
                email: "",
                password: "",
            };
        },
        methods: {
            async handleLogin() {
                if (!this.email || !this.password) {
                    alert("Please enter both email and password.");
                    return;
                }

                try {
                    const response = await apiClient.post("/login/login", {
                        email: this.email,
                        password: this.password,
                    });

                    const { token, userName } = response.data;

                    // Store token & username in localStorage
                    localStorage.setItem("jwtToken", token);
                    localStorage.setItem("userName", userName);

                    // Emit event to update parent component
                    this.$emit("login-success", userName);
                    this.$emit("close");

                    alert("Login successful!");
                } catch (error) {
                    console.error("Login Error:", error);
                    alert(error.response?.data?.message || "Invalid email or password!");
                }
            },
        },
    };</script>

<style scoped>
    /* Modal Styling */
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal {
        background-color: white;
        padding: 20px;
        border-radius: 8px;
        width: 300px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        text-align: center;
    }

        .modal h3 {
            margin-bottom: 20px;
            color: #3f51b5;
        }

    .form-group {
        margin-bottom: 15px;
    }

    .modal input {
        width: 100%;
        padding: 8px;
        border: 1px solid #ccc;
        border-radius: 4px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 8px 12px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        width: 100%;
    }

        .btn-primary:hover {
            background-color: #303f9f;
        }

    .close-btn {
        margin-top: 10px;
        background: none;
        color: #3f51b5;
        border: none;
        cursor: pointer;
        font-size: 14px;
        text-decoration: underline;
    }

        .close-btn:hover {
            color: #1e88e5;
        }
</style>
