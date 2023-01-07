using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    //Range of IDs to take from
    [Range(1,140)]
    public int maxIds;
    //Amount of IDs to store
    [Range(1,140)]
    public int totalIdsToStore;

    //DataList
    public List<int> numbersChosen = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        while (numbersChosen.Count<totalIdsToStore)
        {
            bool isDupe = false;

            int luckyNumber = Random.Range(1, maxIds);

            foreach(int number in numbersChosen)
            {
                if (number == luckyNumber) { isDupe = true; }
            }

            if(!isDupe)
            {
                numbersChosen.Add(luckyNumber);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if(totalIdsToStore>maxIds)
        { totalIdsToStore = maxIds; }
    }
}
