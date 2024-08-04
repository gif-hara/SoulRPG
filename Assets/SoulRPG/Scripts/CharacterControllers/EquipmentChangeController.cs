using SoulRPG.CharacterControllers;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EquipmentChangeController : GamePauseMenuView.IContext
    {
        private readonly Character character;

        private PartType partType;

        public enum PartType
        {
            Weapon1,
            Weapon2,
            Weapon3,
            Weapon4,
            Head,
            Body,
            Arm,
            Leg,
            Accessory1,
            Accessory2,
            Accessory3,
            Accessory4,
        }

        public EquipmentChangeController(Character character, PartType partType)
        {
            this.character = character;
            this.partType = partType;
        }

        public void ChangePartType(PartType partType)
        {
            this.partType = partType;
        }

        public void ChangeEquipment(int itemId)
        {
            switch (partType)
            {
                case PartType.Weapon1:
                    character.Equipment.EquipWeapon(0, itemId);
                    break;
                case PartType.Weapon2:
                    character.Equipment.EquipWeapon(1, itemId);
                    break;
                case PartType.Weapon3:
                    character.Equipment.EquipWeapon(2, itemId);
                    break;
                case PartType.Weapon4:
                    character.Equipment.EquipWeapon(3, itemId);
                    break;
                case PartType.Head:
                    character.Equipment.EquipHead(itemId);
                    break;
                case PartType.Body:
                    character.Equipment.EquipBody(itemId);
                    break;
                case PartType.Arm:
                    character.Equipment.EquipArms(itemId);
                    break;
                case PartType.Leg:
                    character.Equipment.EquipLegs(itemId);
                    break;
                case PartType.Accessory1:
                    character.Equipment.EquipAccessory(0, itemId);
                    break;
                case PartType.Accessory2:
                    character.Equipment.EquipAccessory(1, itemId);
                    break;
                case PartType.Accessory3:
                    character.Equipment.EquipAccessory(2, itemId);
                    break;
                case PartType.Accessory4:
                    character.Equipment.EquipAccessory(3, itemId);
                    break;
            }
        }
    }
}
