using UnityEngine;
using UnityEngine.Serialization;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{

    public class Drop : MonoBehaviour
    {
        [FormerlySerializedAs("DropfirePrefab")] public GameObject dropfirePrefab;
        [FormerlySerializedAs("DropicePrefab")] public GameObject dropicePrefab;
        [FormerlySerializedAs("DroprockPrefab")] public GameObject droprockPrefab;
        [FormerlySerializedAs("DroplightningPrefab")] public GameObject droplightningPrefab;
        [FormerlySerializedAs("DropholyPrefab")] public GameObject dropholyPrefab;

        public void Run(WordVenture.Cards.MagicType magicType1, SelectableObject target, WordVenture.Combat.MagicAffinityTable magicAffinityTable)
        {
            //GameObject target = GameObject.FindGameObjectWithTag(magicType2.ToString());

            GameObject prefabToInstantiate = null;

            switch (magicType1)
            {
                case WordVenture.Cards.MagicType.Fire:
                    prefabToInstantiate = dropfirePrefab;
                    break;
                case WordVenture.Cards.MagicType.Ice:
                    prefabToInstantiate = dropicePrefab;
                    break;
                case WordVenture.Cards.MagicType.Rock:
                    prefabToInstantiate = droprockPrefab;
                    break;
                case WordVenture.Cards.MagicType.Lightning:
                    prefabToInstantiate = droplightningPrefab;
                    break;
                case WordVenture.Cards.MagicType.Holy:
                    prefabToInstantiate = dropholyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                Vector3 instantiatePos = target.transform.position + new Vector3 (0f ,10f ,0f) ;
                GameObject obj =  Instantiate(prefabToInstantiate, instantiatePos , prefabToInstantiate.transform.rotation);
                obj.GetComponent<SpellObj>().InitSpell(MagicType.Drop, magicType1, target, magicAffinityTable);
            }
        }
    }

}
