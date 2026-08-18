import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { decksApi } from '../services/api';

const CreateDeck: React.FC = () => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await decksApi.create({ title, description, isPublic });
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data || 'Failed to create deck');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', padding: '20px' }}>
      <h1>Create New Deck</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Title *</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            style={{ width: '100%', padding: '10px' }}
          />
        </div>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            style={{ width: '100%', padding: '10px', minHeight: '100px' }}
          />
        </div>
        <div style={{ marginBottom: '20px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <input
              type="checkbox"
              checked={isPublic}
              onChange={(e) => setIsPublic(e.target.checked)}
            />
            Make this deck public (anyone can study it)
          </label>
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button type="submit" disabled={loading} style={{ padding: '10px 20px' }}>
            {loading ? 'Creating...' : 'Create Deck'}
          </button>
          <button type="button" onClick={() => navigate('/')} style={{ padding: '10px 20px' }}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreateDeck;
