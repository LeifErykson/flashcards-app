import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { decksApi } from '../services/api';

interface Deck {
  id: number;
  title: string;
  description: string;
  isPublic: boolean;
  ownerId: number;
  ownerUsername: string;
  cardCount: number;
  createdAt: string;
  updatedAt: string;
}

const Dashboard: React.FC = () => {
  const [myDecks, setMyDecks] = useState<Deck[]>([]);
  const [publicDecks, setPublicDecks] = useState<Deck[]>([]);
  const [loading, setLoading] = useState(true);
  const [showMyDecks, setShowMyDecks] = useState(true);

  useEffect(() => {
    loadDecks();
  }, []);

  const loadDecks = async () => {
    try {
      const [myResponse, publicResponse] = await Promise.all([
        decksApi.getAll(),
        decksApi.getPublic(),
      ]);
      setMyDecks(myResponse.data);
      setPublicDecks(publicResponse.data);
    } catch (error) {
      console.error('Error loading decks:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this deck?')) return;
    try {
      await decksApi.delete(id);
      await loadDecks();
    } catch (error) {
      console.error('Error deleting deck:', error);
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>My Decks</h1>
        <Link to="/decks/new">
          <button>Create New Deck</button>
        </Link>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <button
          onClick={() => setShowMyDecks(true)}
          style={{
            padding: '10px 20px',
            background: showMyDecks ? '#007bff' : '#f0f0f0',
            color: showMyDecks ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer',
            marginRight: '10px',
          }}
        >
          My Decks ({myDecks.length})
        </button>
        <button
          onClick={() => setShowMyDecks(false)}
          style={{
            padding: '10px 20px',
            background: !showMyDecks ? '#007bff' : '#f0f0f0',
            color: !showMyDecks ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer',
          }}
        >
          Public Decks ({publicDecks.length})
        </button>
      </div>

      <div>
        {showMyDecks ? (
          myDecks.length === 0 ? (
            <p>You haven't created any decks yet.</p>
          ) : (
            <DeckList decks={myDecks} onDelete={handleDelete} showOwner={false} />
          )
        ) : (
          publicDecks.length === 0 ? (
            <p>No public decks available.</p>
          ) : (
            <DeckList decks={publicDecks} onDelete={() => {}} showOwner={true} />
          )
        )}
      </div>
    </div>
  );
};

interface DeckListProps {
  decks: Deck[];
  onDelete: (id: number) => void;
  showOwner: boolean;
}

const DeckList: React.FC<DeckListProps> = ({ decks, onDelete, showOwner }) => {
  return (
    <div>
      {decks.map((deck) => (
        <div
          key={deck.id}
          style={{
            border: '1px solid #ddd',
            padding: '15px',
            marginBottom: '15px',
            borderRadius: '5px',
          }}
        >
          <h3>{deck.title}</h3>
          <p>{deck.description}</p>
          <p style={{ fontSize: '14px', color: '#666' }}>
            {deck.cardCount} cards • {deck.isPublic ? 'Public' : 'Private'}
            {showOwner && ` • Created by ${deck.ownerUsername}`}
          </p>
          <div>
            <Link to={`/decks/${deck.id}/study`}>
              <button style={{ marginRight: '10px' }}>Study</button>
            </Link>
            <Link to={`/decks/${deck.id}/edit`}>
              <button style={{ marginRight: '10px' }}>Edit</button>
            </Link>
            {onDelete !== (() => {}) && (
              <button onClick={() => onDelete(deck.id)} style={{ color: 'red' }}>
                Delete
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

export default Dashboard;
