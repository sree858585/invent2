import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'http://localhost:51466/api', // Ensure you're using the correct backend URL
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true, // Enables CORS with credentials
});

export default apiClient;
