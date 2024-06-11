namespace SimpleSudokuDemo.Core;

public interface IAbstractFactory<T>
{
    T Create();
}