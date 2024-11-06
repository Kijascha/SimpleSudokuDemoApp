using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SimpleSudoku.CommonLibrary.Models
{
    public class Candidates : INotifyPropertyChanged
    {
        private int _bitMask;
        private readonly ObservableCollection<int> _collection;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Candidates()
        {
            _bitMask = 0b111111111;
            _collection = [];
            UpdateCollectionInternal();
        }
        public Candidates(int bitMask)
        {
            _bitMask = bitMask;
            _collection = [];
            UpdateCollectionInternal();
        }
        public int BitMask
        {
            get => _bitMask;
            set => _bitMask = value;
        }
        public Candidates(HashSet<int> candidates)
        {
            _bitMask = 0;
            _collection = [];
            FromHashSet(candidates);
        }

        public void Clear()
        {
            _bitMask = 0;
            UpdateCollection();
        }

        public void Add(int candidate)
        {
            if (candidate < 1 || candidate > 9)
                throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

            _bitMask |= (1 << (candidate - 1)); // Set the bit for the candidate
            UpdateCollection();
        }

        public bool Remove(int candidate)
        {
            if (candidate < 1 || candidate > 9)
                throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

            int mask = 1 << (candidate - 1);

            // Check if the bit is already cleared
            if ((_bitMask & mask) == 0)
            {
                return false; // Candidate was not present
            }

            _bitMask &= ~mask; // Clear the bit for the candidate
            UpdateCollection(); // Update the ObservableCollection<int>

            return true; // Candidate was successfully removed
        }

        public bool Contains(int candidate)
        {
            if (candidate < 1 || candidate > 9)
                throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

            return (_bitMask & (1 << (candidate - 1))) != 0; // Check the bit for the candidate
        }

        public ObservableCollection<int> Collection
        {
            get { return _collection; }
        }
        // New method to set candidates from a bitmask
        public void FromBitMask(int bitmask)
        {
            _bitMask = bitmask;
            UpdateCollection();  // Update the ObservableCollection<int> based on the new bitmask
        }
        public void FromHashSet(HashSet<int> candidates)
        {
            _bitMask = 0;
            foreach (int candidate in candidates)
            {
                if (candidate < 1 || candidate > 9)
                    throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

                _bitMask |= (1 << (candidate - 1)); // Set the bit for the candidate
            }
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            UpdateCollectionInternal();
            OnPropertyChanged(nameof(_bitMask));   // Notify UI that bitmask has changed
        }

        private void UpdateCollectionInternal()
        {
            _collection.Clear(); // Clear existing collection

            for (int i = 1; i <= 9; i++)
            {
                if (Contains(i))
                {
                    _collection.Add(i); // Automatically notifies UI of each addition
                }
            }

            // Notify UI that the collection has changed (if necessary)
            OnPropertyChanged(nameof(Collection));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Convert.ToString(_bitMask, 2).PadLeft(9, '0'); // PadLeft 9 to include all bits
        }
    }
}
