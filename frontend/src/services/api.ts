import axios from 'axios';

const API_BASE_URL = 'http://localhost:5279/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth endpoints
export const authApi = {
  register: (data: { username: string; email: string; password: string }) =>
    api.post('/auth/register', data),
  login: (data: { email: string; password: string }) =>
    api.post('/auth/login', data),
};

// Deck endpoints
export const decksApi = {
  getAll: () => api.get('/decks'),
  getPublic: () => api.get('/decks/public'),
  getById: (id: number) => api.get(`/decks/${id}`),
  create: (data: { title: string; description: string; isPublic: boolean }) =>
    api.post('/decks', data),
  update: (id: number, data: { title: string; description: string; isPublic: boolean }) =>
    api.put(`/decks/${id}`, data),
  delete: (id: number) => api.delete(`/decks/${id}`),
};

// Flashcard endpoints
export const flashcardsApi = {
  getByDeck: (deckId: number) => api.get(`/flashcards/deck/${deckId}`),
  create: (deckId: number, data: { front: string; back: string }) =>
    api.post(`/flashcards/deck/${deckId}`, data),
  update: (id: number, data: { front: string; back: string }) =>
    api.put(`/flashcards/${id}`, data),
  delete: (id: number) => api.delete(`/flashcards/${id}`),
};

// Study endpoints
export const studyApi = {
  resetProgress: (deckId: number) => api.post(`/study/reset/${deckId}`),
  start: (deckId: number) => api.post('/study/start', { deckId }),
  record: (data: { cardId: number; isCorrect: boolean }) =>
    api.post('/study/record', data),
  progress: (deckId: number) => api.get(`/study/progress/${deckId}`),
};

// Study Settings endpoints
export const settingsApi = {
  get: (deckId: number) => api.get(`/studysettings/deck/${deckId}`),
  update: (deckId: number, data: { shuffleCards: boolean; correctAnswersRequired: number }) =>
    api.put(`/studysettings/deck/${deckId}`, data),
};

export default api;
