using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
public class DialogController : MonoBehaviour
{   [SerializeField]
    [Tooltip("Assign DialogLarge_192x192.prefab")]
    private GameObject dialogPrefabLarge;
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }
    public void OpenConfirmationDialogLarge()
    {
        Dialog.Open(DialogPrefabLarge, DialogButtonType.OK, "Confirmation Dialog, Large, Far", "This is an example of a large dialog with only one button, placed at far interaction range", false);
    }
    public void OpenChoiceDialogLarge()
    {
        Dialog myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Yes | DialogButtonType.No, "Choice Dialog, Large, Near", "This is an example of a large dialog with a choice message for the user, placed at near interaction range", true);
        if (myDialog != null)
        {
            myDialog.OnClosed += OnClosedDialogEvent;
        }
    }
    private void OnClosedDialogEvent(DialogResult obj)
    {
        if (obj.Result == DialogButtonType.Yes)
        {
            Debug.Log(obj.Result);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TodoWork(){
        print("click!!");
    }
}
