using PleasantUI.Controls;

namespace PleasantUI.Interfaces;

public interface IPleasantWindowModal
{
    void AddModalWindow(ModalWindow modalWindow);
    void RemoveModalWindow(ModalWindow modalWindow);
}