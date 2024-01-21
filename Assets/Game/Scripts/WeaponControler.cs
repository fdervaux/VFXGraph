using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControler : MonoBehaviour
{
    private int _ammoLeft = 0;

    [SerializeField]
    private int _magazineSize = 6;
    private Animator _playerAnimator;

    public void decreaseAmmo()
    {
        if(_ammoLeft > 0)
            _ammoLeft--;
    }

    public void reloadAmmo()
    {
        _ammoLeft = _magazineSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ammoLeft = _magazineSize;
        _playerAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _playerAnimator.SetLayerWeight(_playerAnimator.GetLayerIndex("OutOfAmmo"), _ammoLeft > 0 ? 0.0f : 1.0f);
    }
}
