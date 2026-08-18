import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studyApi, decksApi } from '../services/api';

interface StudyCard {
  cardId: number;
  front: string;
  back: string;
  isMastered: boolean;
}

interface StudySession {
  sessionId: number;
  cards: StudyCard[];
  totalCards: number;
  masteredCount: number;
}

const Study: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [session, setSession] = useState<StudySession | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [deckTitle, setDeckTitle] = useState('');
  const [completed, setCompleted] = useState(false);

  useEffect(() => {
    startStudy();
  }, [id]);

  const startStudy = async () => {
    try {
      const [deckRes, studyRes] = await Promise.all([
        decksApi.getById(Number(id)),
        studyApi.start(Number(id))
      ]);
      
      setDeckTitle(deckRes.data.title);
      setSession(studyRes.data);
      setCurrentIndex(0);
      setIsFlipped(false);
    } catch (err: any) {
      setError(err.response?.data || 'Failed to start study session');
    } finally {
      setLoading(false);
    }
  };

  const handleCardResult = async (isCorrect: boolean) => {
    if (!session || !session.cards[currentIndex]) return;
    
    const card = session.cards[currentIndex];
    await studyApi.record({ cardId: card.cardId, isCorrect });
    
    // Move to next card
    const nextIndex = currentIndex + 1;
    if (nextIndex >= session.cards.length) {
      setCompleted(true);
    } else {
      setCurrentIndex(nextIndex);
      setIsFlipped(false);
    }
  };

  const handleFlip = () => {
    setIsFlipped(!isFlipped);
  };

  if (loading) return <div>Loading study session...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;
  if (completed || !session || session.cards.length === 0) {
    return (
      <div style={{ maxWidth: '600px', margin: '50px auto', textAlign: 'center' }}>
        <h2>🎉 Study Complete!</h2>
        <p>You've reviewed all cards in this deck.</p>
        <p>
          Mastered: {session?.masteredCount || 0} / {session?.totalCards || 0}
        </p>
        <button onClick={() => navigate('/')} style={{ padding: '10px 20px', marginTop: '20px' }}>
          Back to Dashboard
        </button>
      </div>
    );
  }

  const currentCard = session.cards[currentIndex];
  const progress = ((currentIndex) / session.cards.length) * 100;

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h2>{deckTitle}</h2>
        <button onClick={() => navigate('/')}>Exit Study</button>
      </div>
      
      {/* Progress bar */}
      <div style={{ marginBottom: '20px' }}>
        <div style={{ height: '8px', background: '#e0e0e0', borderRadius: '4px', overflow: 'hidden' }}>
          <div style={{ height: '100%', width: `${progress}%`, background: '#007bff' }} />
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '5px' }}>
          <span>{currentIndex + 1} / {session.cards.length}</span>
          <span>{Math.round(progress)}%</span>
        </div>
      </div>
      
      {/* Flashcard */}
      <div
        onClick={handleFlip}
        style={{
          perspective: '1000px',
          cursor: 'pointer',
          minHeight: '300px',
          marginBottom: '20px'
        }}
      >
        <div
          style={{
            transform: isFlipped ? 'rotateY(180deg)' : 'rotateY(0deg)',
            transformStyle: 'preserve-3d',
            transition: 'transform 0.5s',
            position: 'relative',
            width: '100%',
            minHeight: '300px'
          }}
        >
          {/* Front */}
          <div
            style={{
              position: 'absolute',
              width: '100%',
              minHeight: '300px',
              backfaceVisibility: 'hidden',
              border: '2px solid #007bff',
              borderRadius: '10px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              padding: '20px',
              background: 'white',
              fontSize: '24px',
              textAlign: 'center'
            }}
          >
            <div>
              <div style={{ fontSize: '14px', color: '#666', marginBottom: '20px' }}>
                Question {currentIndex + 1}
              </div>
              {currentCard.front}
            </div>
          </div>
          
          {/* Back */}
          <div
            style={{
              position: 'absolute',
              width: '100%',
              minHeight: '300px',
              backfaceVisibility: 'hidden',
              transform: 'rotateY(180deg)',
              border: '2px solid #28a745',
              borderRadius: '10px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              padding: '20px',
              background: '#f8fff8',
              fontSize: '24px',
              textAlign: 'center'
            }}
          >
            <div>
              <div style={{ fontSize: '14px', color: '#666', marginBottom: '20px' }}>
                Answer
              </div>
              {currentCard.back}
            </div>
          </div>
        </div>
      </div>
      
      <div style={{ textAlign: 'center', fontSize: '14px', color: '#666', marginBottom: '20px' }}>
        Click the card to flip
      </div>
      
      {isFlipped && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '20px' }}>
          <button
            onClick={() => handleCardResult(false)}
            style={{
              padding: '12px 30px',
              backgroundColor: '#dc3545',
              color: 'white',
              border: 'none',
              borderRadius: '5px',
              cursor: 'pointer',
              fontSize: '16px'
            }}
          >
            ❌ Incorrect
          </button>
          <button
            onClick={() => handleCardResult(true)}
            style={{
              padding: '12px 30px',
              backgroundColor: '#28a745',
              color: 'white',
              border: 'none',
              borderRadius: '5px',
              cursor: 'pointer',
              fontSize: '16px'
            }}
          >
            ✅ Correct
          </button>
        </div>
      )}
    </div>
  );
};

export default Study;
