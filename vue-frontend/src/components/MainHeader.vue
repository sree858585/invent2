<template>
    <header>
        <nav class="navbar navbar-custom">
            <div class="container navbar-container">
                <div class="navbar-brand">
                    Department of Health
                </div>
                <div class="navbar-links">
                    <partial name="_LoginPartial" />
                    <div class="user-greeting" v-if="user">
                        <img src="@/assets/img.gif"
                                             alt="User Icon"
                                             class="user-icon" />
                        Hello, {{ user.firstName }}!
                    </div>
                </div>
            </div>
        </nav>
    </header>
</template>

<script>import apiClient from "@/axios.js";

    export default {
        name: "MainHeader",
        data() {
            return {
                user: null, // Placeholder for user data
            };
        },

        created() {
            this.fetchUser();
        },

        methods: {
            async fetchUser() {
                try {
                    const userId = localStorage.getItem('userId'); // Get the user ID from local storage
                    const response = await apiClient.get(`/user/${userId}`); // Use Axios instance to make the request

                    this.user = response.data; // Set user data from the response
                } catch (error) {
                    console.error('Error fetching user data:', error);
                }
            }
        }
    }</script>

<style scoped>

    /* Styles for the navbar */
      .navbar-custom {
            background-color: #6e528d; /* Background color */
            color: white; /* Font color */
            padding: 15px 25px; /* Padding to maintain navbar size */
            height: 20px; /* Set fixed height for the navbar */

    }


      .navbar-container {
            display: flex; /* Flex layout for the navbar */
            align-items: center; /* Center items vertically */
            justify-content: space-between; /* Space between brand and links */
            width: 100%; /* Take full width */

    }


      .navbar-brand {
            font-size: 20px; /* Font size */
            color: white !important; /* Font color */
            font-weight: bold; /* Make the brand title bold */
            padding-left: 25px; /* Padding on the left */

    }


      .navbar-links {
            display: flex; /* Flex layout for links */
            align-items: center; /* Center items vertically */

    }


      .user-greeting {
            color: white; /* Greeting color */
            font-size: 16px; /* Font size for greeting */
            display: flex; /* Flex container for the greeting */
            align-items: center; /* Center icon and text vertically */
            margin-left: auto; /* Push it to the right */
            position: relative; /* Ensure positioning context */

    }


      .user-icon {
            width: 120px; /* Set width for the icon */
            height: 120px; /* Set height for the icon */
            margin-right: 3px; /* Space between icon and greeting */
            margin-top: -65px; /* Adjust as needed to raise the icon */
            margin-bottom: -30px;

    }
</style>