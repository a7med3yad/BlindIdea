# Frontend API Examples

Base URL: `https://localhost:7024`

## 1. Register

```javascript
// Using fetch
const response = await fetch("https://localhost:7024/api/auth/register", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  credentials: "include",
  body: JSON.stringify({
    email: "test@test.com",
    password: "Password123!",
    firstName: "Test",
    lastName: "User",
    userName: "testuser"
  })
});
const data = await response.json();

// Using axios
const { data } = await axios.post(
  "https://localhost:7024/api/auth/register",
  {
    email: "test@test.com",
    password: "Password123!",
    firstName: "Test",
    lastName: "User",
    userName: "testuser"
  },
  { withCredentials: true }
);
```

## 2. Login

```javascript
// Using fetch
const response = await fetch("https://localhost:7024/api/auth/login", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  credentials: "include",
  body: JSON.stringify({
    emailOrUserName: "test@test.com",
    password: "Password123!"
  })
});
const data = await response.json();
const token = data.data?.accessToken;

// Using axios
const { data } = await axios.post(
  "https://localhost:7024/api/auth/login",
  {
    emailOrUserName: "test@test.com",
    password: "Password123!"
  },
  { withCredentials: true }
);
const token = data.data?.accessToken;
```

## 3. Authenticated Request (GET ideas)

```javascript
const token = "YOUR_JWT_TOKEN";

// Using fetch
const response = await fetch("https://localhost:7024/api/ideas", {
  headers: {
    "Authorization": `Bearer ${token}`
  },
  credentials: "include"
});
const ideas = await response.json();

// Using axios
const { data } = await axios.get("https://localhost:7024/api/ideas", {
  headers: {
    Authorization: `Bearer ${token}`
  },
  withCredentials: true
});
```

## 4. Authenticated Request (POST idea)

```javascript
const token = "YOUR_JWT_TOKEN";

// Using axios
const { data } = await axios.post(
  "https://localhost:7024/api/ideas",
  {
    title: "My New Idea",
    description: "A detailed description of my idea.",
    isAnonymous: false,
    teamId: null
  },
  {
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    },
    withCredentials: true
  }
);
```

## 5. Complete Axios Instance (Recommended)

```javascript
import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7024",
  withCredentials: true,
  headers: { "Content-Type": "application/json" }
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Login and store token
const login = async (emailOrUserName, password) => {
  const { data } = await api.post("/api/auth/login", {
    emailOrUserName,
    password
  });
  const token = data.data?.accessToken;
  if (token) localStorage.setItem("token", token);
  return data;
};

// Get ideas
const getIdeas = () => api.get("/api/ideas");

// Create idea
const createIdea = (idea) => api.post("/api/ideas", idea);
```
