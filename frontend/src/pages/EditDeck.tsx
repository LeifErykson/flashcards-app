// Full file with Back button included
import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { decksApi, flashcardsApi, studyApi } from '../services/api';

interface Flashcard {
  id: number;
  front: string;
  back: string;
  deckId: number;
}

interface Deck {
  id: number;
  title: string;
  description: string;
  isPublic: boolean;
}

const EditDeck: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [deck, setDeck] = useState<Deck | null>(null);
  const [flashcards, setFlashcards] = useState<Flashcard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(false);
  
  const [newFront, setNewFront] = useState('');
  const [newBack, setNewBack] = useState('');
  const [editingCard, setEditingCard] = useState<Flashcard | null>(null);

  useEffect(() => {
    loadDeckData();
  }, [id]);

  const loadDeckData = async () => {
    try {
      const [deckRes, cardsRes] = await Promise.all([
        decksApi.getById(Number(id)),
        flashcardsApi.getByDeck(Number(id))
      ]);
      
      const deckData = deckRes.data;
      setDeck(deckData);
      setTitle(deckData.title);
      setDescription(deckData.description);
      setIsPublic(deckData.isPublic);
      setFlashcards(cardsRes.data);
    } catch (err: any) {
      setError(err.response?.data || 'Failed to load deck');
    } finally {
      setLoading(false);
    }
  };

  const updateDeck = async () => {
    try {
      await decksApi.update(Number(id), { title, description, isPublic });
      await loadDeckData();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to update deck');
    }
  };

  const addFlashcard = async () => {
    if (!newFront.trim() || !newBack.trim()) return;
    
    try {
      await flashcardsApi.create(Number(id), { front: newFront, back: newBack });
      setNewFront('');
      setNewBack('');
      await loadDeckData();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to add flashcard');
    }
  };

  const updateFlashcard = async () => {
    if (!editingCard) return;
    
    try {
      await flashcardsApi.update(editingCard.id, {
        front: editingCard.front,
        back: editingCard.back
      });
      setEditingCard(null);
      await loadDeckData();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to update flashcard');
    }
  };

  const deleteFlashcard = async (cardId: number) => {
    if (!window.confirm('Delete this flashcard?')) return;
    
    try {
      await flashcardsApi.delete(cardId);
      await loadDeckData();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to delete flashcard');
    }
  };

  const deleteDeck = async () => {
    if (!window.confirm('Delete this deck and all its flashcards?')) return;
    
    try {
      await decksApi.delete(Number(id));
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data || 'Failed to delete deck');
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;
  if (!deck) return <div>Deck not found</div>;

  const resetProgress = async () => {
    if (!window.confirm('Reset all progress for this deck? This will mark all cards as unmastered.')) return;
    
    try {
      // Get all flashcards, then reset progress for each
      const cardsRes = await flashcardsApi.getByDeck(Number(id));
      const cards = cardsRes.data;
      
      for (const card of cards) {
        await studyApi.record({ cardId: card.id, isCorrect: false });
      }
      
      alert('Progress reset successfully!');
      await loadDeckData();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to reset progress');
    }
  };
  // Add this button next to the Update/Delete buttons:
  <button onClick={resetProgress} style={{ marginRight: '10px', backgroundColor: '#ffc107' }}>
    Reset Progress
  </button>


  return (
    <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1>Edit Deck</h1>
        <button onClick={() => navigate('/')} style={{ padding: '8px 16px', cursor: 'pointer' }}>
          ← Back to Dashboard
        </button>
      </div>
      
      {/* Deck Info */}
      <div style={{ border: '1px solid #ddd', padding: '20px', borderRadius: '5px', marginBottom: '20px' }}>
        <h2>Deck Info</h2>
        <div style={{ marginBottom: '10px' }}>
          <label>Title *</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            style={{ width: '100%', padding: '8px', marginTop: '5px' }}
          />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            style={{ width: '100%', padding: '8px', marginTop: '5px', minHeight: '60px' }}
          />
        </div>
        <div style={{ marginBottom: '10px' }}>
          <label>
            <input
              type="checkbox"
              checked={isPublic}
              onChange={(e) => setIsPublic(e.target.checked)}
            />
            Public deck
          </label>
        </div>
        <button onClick={updateDeck} style={{ marginRight: '10px' }}>Update Deck</button>
        <button onClick={deleteDeck} style={{ backgroundColor: '#dc3545', color: 'white' }}>Delete Deck</button>
      </div>
      
      {/* Flashcards */}
      <div>
        <h2>Flashcards ({flashcards.length})</h2>
        
        {/* Add Flashcard Form */}
        <div style={{ border: '1px solid #ddd', padding: '15px', borderRadius: '5px', marginBottom: '20px' }}>
          <h3>Add New Flashcard</h3>
          <div style={{ marginBottom: '10px' }}>
            <label>Front (Question)</label>
            <input
              type="text"
              value={newFront}
              onChange={(e) => setNewFront(e.target.value)}
              style={{ width: '100%', padding: '8px', marginTop: '5px' }}
              placeholder="e.g., What is the capital of France?"
            />
          </div>
          <div style={{ marginBottom: '10px' }}>
            <label>Back (Answer)</label>
            <input
              type="text"
              value={newBack}
              onChange={(e) => setNewBack(e.target.value)}
              style={{ width: '100%', padding: '8px', marginTop: '5px' }}
              placeholder="e.g., Paris"
            />
          </div>
          <button onClick={addFlashcard}>Add Flashcard</button>
        </div>
        
        {/* Flashcard List */}
        {flashcards.length === 0 ? (
          <p>No flashcards yet. Add one above!</p>
        ) : (
          <div>
            {flashcards.map((card) => (
              <div
                key={card.id}
                style={{
                  border: '1px solid #ddd',
                  padding: '15px',
                  borderRadius: '5px',
                  marginBottom: '10px'
                }}
              >
                {editingCard?.id === card.id ? (
                  <div>
                    <div style={{ marginBottom: '10px' }}>
                      <label>Front</label>
                      <input
                        type="text"
                        value={editingCard.front}
                        onChange={(e) => setEditingCard({ ...editingCard, front: e.target.value })}
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                      />
                    </div>
                    <div style={{ marginBottom: '10px' }}>
                      <label>Back</label>
                      <input
                        type="text"
                        value={editingCard.back}
                        onChange={(e) => setEditingCard({ ...editingCard, back: e.target.value })}
                        style={{ width: '100%', padding: '8px', marginTop: '5px' }}
                      />
                    </div>
                    <button onClick={updateFlashcard} style={{ marginRight: '10px' }}>Save</button>
                    <button onClick={() => setEditingCard(null)}>Cancel</button>
                  </div>
                ) : (
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                      <strong>Q:</strong> {card.front}
                      <br />
                      <strong>A:</strong> {card.back}
                    </div>
                    <div>
                      <button onClick={() => setEditingCard(card)} style={{ marginRight: '10px' }}>Edit</button>
                      <button onClick={() => deleteFlashcard(card.id)} style={{ color: 'red' }}>Delete</button>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default EditDeck;