const BASE_URL = ""; 

async function apiFetch(endpoint, method = "GET", data = null) {
  const url = `${BASE_URL}${endpoint}`;

  const headers = {
    "Content-Type": "application/json",
  };

  const options = {
    method,
    headers,
    credentials: "include", 
  };

  if (data) {
    options.body = JSON.stringify(data);
  }

  const response = await fetch(url, options);

  if (response.status === 401) {
    throw new Error("Unauthorized: Missing or invalid token");
  }

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`API request failed: ${errorText}`);
  }

  return response.json();
}

export async function apiGet(endpoint) {
  return apiFetch(endpoint, "GET");
}

export async function apiPost(endpoint, data) {
  return apiFetch(endpoint, "POST", data);
}

export async function apiPut(endpoint, data) {
  return apiFetch(endpoint, "PUT", data);
}

export async function apiDelete(endpoint) {
  return apiFetch(endpoint, "DELETE");
}
