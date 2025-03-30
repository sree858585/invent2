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
import eventBus from "@/eventBus.js"; // ✅ Ensure eventBus is imported

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
                console.log("🔄 Attempting login with:", { email: this.email });

                const response = await apiClient.post("/login/login", {
                    email: this.email,
                    password: this.password,
                });

                console.log("🔥 API Response:", response.data);

                if (!response.data || !response.data.userId) {  // ✅ Ensure userId (GUID) exists
                    alert("⚠️ Login successful, but user data is missing.");
                    console.error("🚨 User ID is missing in response:", response.data);
                    return;
                }

                this.handleLoginSuccess(response.data);

            } catch (error) {
                console.error("❌ Login Error:", error);
                alert(error.response?.data?.message || "Invalid email or password!");
            }
        },

        handleLoginSuccess(userData) {
    console.log("🔥 Full Login Response:", userData);

    if (!userData || !userData.userId) {  
        alert("⚠️ Login successful, but user data is missing.");
        return;
    }

    // 🔥 Ensure userId is a GUID, not an integer
    if (typeof userData.userId !== "string" || userData.userId.length < 36) {
        alert("⚠️ User ID format is incorrect. Expected GUID but got: " + userData.userId);
        console.error("🚨 Incorrect User ID:", userData.userId);
        return;
    }

    // ✅ Store Correct UserId (GUID) in localStorage
    localStorage.setItem("userId", userData.userId);  
    localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
    localStorage.setItem("jwtToken", userData.token);

    console.log("✅ Stored user details:", {
        userId: localStorage.getItem("userId"),
        userName: localStorage.getItem("userName"),
        jwtToken: localStorage.getItem("jwtToken"),
    });

    eventBus.emit("auth-change");  // ✅ Notify other components
    this.$emit("login-success", userData);
    this.$emit("close");
    this.reloadPage();
},

        reloadPage() {
            setTimeout(() => {
                window.location.reload(); // 🔄 Reload to reflect changes
            }, 500);
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
