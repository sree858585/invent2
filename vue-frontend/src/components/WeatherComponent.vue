<template>
    <div>
        <h2>Weather Forecast</h2>
        <ul v-if="weatherData.length">
            <li v-for="(forecast, index) in weatherData" :key="index">
                <p><strong>Date:</strong> {{ forecast.date }}</p>
                <p><strong>Temperature (°C):</strong> {{ forecast.temperatureC }}</p>
                <p><strong>Summary:</strong> {{ forecast.summary }}</p>
            </li>
        </ul>
        <p v-else>Loading weather data...</p>
    </div>
</template>

<script>import axios from "axios";

    export default {
        name: "WeatherComponent",
        data() {
            return {
                weatherData: [], // To store weather data fetched from the API
            };
        },
        methods: {
            async fetchWeatherData() {
                try {
                    console.log("Fetching weather data...");
                    const response = await axios.get("https://localhost:7190/weatherforecast");
                    console.log("API Response:", response.data); // Log the response
                    this.weatherData = response.data;
                } catch (error) {
                    console.error("Error fetching weather data:", error);
                    alert("Failed to load weather data. Check console for details.");
                }
            },
        },
        mounted() {
            this.fetchWeatherData(); // Fetch data when the component is mounted
        },
    };</script>


<style scoped>
    h2 {
        color: #1e88e5;
    }

    ul {
        list-style: none;
        padding: 0;
    }

    li {
        margin-bottom: 15px;
        padding: 10px;
        border: 1px solid #ddd;
        border-radius: 8px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

        li p {
            margin: 5px 0;
            font-size: 1rem;
        }
</style>
