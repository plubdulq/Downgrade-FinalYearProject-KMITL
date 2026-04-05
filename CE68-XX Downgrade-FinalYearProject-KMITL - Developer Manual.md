# Developer Manual: VR Server Room Simulation (Project Analysis)

## 1) Executive Overview

**ภาพรวมระดับสูงของโปรเจกต์ (High-Level Overview)**
ระบบสถาปัตยกรรมนี้คือ "แอปพลิเคชัน VR สำหรับจำลองสภาพแวดล้อมระบบเครือข่ายห้องเซิร์ฟเวอร์ (Server Room Network Simulation)" พัฒนาด้วย Unity Engine (C#) ผู้ใช้สามารถใช้แว่น VR (เช่น Meta Quest ผ่านระบบ BNG Framework หรือ Oculus Integration) ในการหยิบจับอุปกรณ์เครือข่าย (เช่น สวิตช์, เคเบิล, สายไฟ) และทำการเชื่อมต่อพอร์ตต่าง ๆ อย่างสมจริง ระบบจะทำการคำนวณโครงข่าย (Network Topology) และแสดงสถานะการเชื่อมต่อ การจัดเก็บข้อมูล รวมถึงปุ่มคำสั่งควบคุมผ่าน UI แบบโฮโลแกรมภายในโลกเสมือน

**การแบ่ง Subsystems หลัก (Core Subsystems)**
ระบบแบ่งแยกการทำงานออกเป็นระบบย่อยที่ทำงานประสานกันดังนี้:
1. **Interaction & VR Core System:** จัดการรับ Input จาก Controller (ผ่าน BNG Framework) สำหรับการหยิบ จับ ชี้ (Raycast/Pointing) และการ Snap อุปกรณ์เข้าสู่จุดเซ็ตอัป (SnapZone)
2. **Cable & Port Management System (Chomm.CableSystem):** แกนหลักของโปรเจกต์ จัดการตรรกะหัวเสียบ (Plug) และช่องเสียบ (Port) การเรนเดอร์เส้นสายเคเบิลแบบไดนามิก และตรวจสอบประเภทสายที่เสียบ (Fiber, Copper, Power)
3. **Simulation & Network Logic System (PLUB / Network):** ตรวจสอบเครือข่ายที่เชื่อมต่อว่าลิงก์สำเร็จหรือไม่ อุปกรณ์ได้รับไฟหรือไม่ รวมถึง Heat/Cooling Simulation
4. **Save/Load & State System:** จัดการ Serialization โครงสร้างห้อง การจัดเก็บ Snapshot สถานะอุปกรณ์ อุณหภูมิ และสายเคเบิลลงไฟล์ JSON หรือโครงสร้างข้อมูล Persistent
5. **UI & Monitoring System:** แสดงข้อมูล (CableDataUI, DropDownContentControl) และให้ผู้เล่นตอบโต้กับระบบผ่านแผงคอนโซลเสมือน

**ความสัมพันธ์ระหว่าง Subsystem (Subsystem Relationships)**
VR Core ส่ง Input ให้ชิ้นส่วน -> Cable System จับสัญญาณการเสียบ (OnTrigger/Snap) -> วิ่งไปอัปเดต State System ว่าจุดเชื่อมต่อสำเร็จ -> Network Logic System ทำงานเพื่อประเมินสถานะของลิงก์ -> สุดท้ายตีกลับมาให้ UI System เรนเดอร์ข้อมูลว่าระบบทำงานปกติหรือล้มเหลว

---

## 2) Project Structure Overview (เจาะลึกโครงสร้างไฟล์และโฟลเดอร์)

โครงสร้างไฟล์และโฟลเดอร์ในสถาปัตยกรรมของโปรเจกต์ (เน้นที่ระดับ `Assets/Chomm/` ซึ่งเป็นระบบหลัก) โครงสร้างนี้ถูกจัดเก็บแยกเป็นหมวดหมู่เพื่อให้ง่ายต่อการพัฒนาต่อยอด:

```text
Assets/
├── Chomm/                             # ศูนย์กลางระบบหลักทั้งหมด (Core Systems)
│   ├── CableDataUI.cs                 # สคริปต์จัดการ UI ข้อมูลของสายเคเบิล
│   ├── DetectControllerPointing.cs    # ตรวจจับการชี้ของคอนโทรลเลอร์ VR
│   ├── EquipmentData.cs               # จัดเก็บข้อมูลของอุปกรณ์แต่ละตัว (รุ่น, สเปค)
│   ├── GameSaveData.cs                # โครงสร้างข้อมูล (Data Model) สำหรับการ Save/Load
│   ├── SaveManager.cs                 # ระบบบันทึกและโหลดสถานะห้องและอุปกรณ์
│   ├── SceneLoader.cs                 # เปลี่ยน Scene ภายในเกมเพลย์
│   ├── ScreenshotManager.cs           # จัดการระบบถ่ายภาพหน้าจอจำลอง
│   │
│   ├── CableSystem/                   # ระบบแกนกลางเกี่ยวกับฟิสิกส์และการเสียบสาย
│   │   ├── Cable.cs                   # ตรรกะของเส้นสายเคเบิล การโค้งงอเส้นสาย
│   │   ├── CablePlug.cs               # ตรรกะของหัว Plug ทุกชนิด (ตรวจสอบสถานะเสียบ/ถอด)
│   │   ├── CableSpawner.cs            # ระบบเสกสายเคเบิลออกมาจากจุดเก็บ
│   │   └── Port.cs                    # ตรรกะของพอร์ต (ช่องรับสัญญาณ) รับค่าการเชื่อมต่อ
│   │
│   ├── Environment/                   # ระบบสิ่งแวดล้อมจำลองภายในห้องเซิร์ฟเวอร์
│   │   ├── RoomGenerator.cs           # สร้างหรือขยายขนาดห้องตามเงื่อนไข (Procedural/Dynamic)
│   │   └── SlidingDoorController.cs   # ควบคุมแอนิเมชันเปิด-ปิดประตูอัตโนมัติ
│   │
│   ├── Scenes/                        # ไฟล์ Scene ของ Unity
│   │   ├── MainMenuScene.unity        # หน้าจอเข้าเกมหรือเลือกห้องเซิร์ฟเวอร์
│   │   ├── PlayScene.unity            # ห้องเซิร์ฟเวอร์หลักสำหรับการจำลอง
│   │   └── TestScene.unity            # สำหรับทดสอบระบบแบบเจาะจง (แซนด์บ็อกซ์)
│   │
│   ├── ScriptableObjects/             # ข้อมูลโครงสร้างตั้งต้นที่ฝังในแอสเซท
│   │   └── RoomTemplate.cs            # Template ข้อมูลขนาดห้องและการจัดวางพื้นฐาน
│   │
│   ├── UI/                            # ควบคุมพาเนล 2D/3D โฮโลแกรมภายใน VR
│   │   ├── MainMenuManager.cs         # จัดการปุ่มและเมนูใน MainMenuScene
│   │   ├── MyRoomManager.cs           # จัดการเซฟห้องส่วนตัวของผู้เล่น
│   │   └── RoomCreatorManager.cs      # ระบบสร้างห้องเซิร์ฟเวอร์ใหม่
│   │
│   └── (Prefabs ต่างๆ เช่น Cable, Plug, Port) # ชิ้นส่วน 3D ที่ประกอบไว้แล้ว
│
├── Scripts/                           # โค้ดส่วนระบบร่วมอื่นๆ (โมดูลเพิ่มเติมจากทีม)
│   ├── DRAGON/                        # (โมดูลด้าน CCTV, สัญญาณเตือนอัคคีภัย และความปลอดภัย)
│   │   ├── CCTV/                      # สคริปต์ตรวจจับผู้เล่น และกล้องวงจรปิด
│   │   └── Fire/                      # สคริปต์แจ้งเตือนไฟไหม้ และระบบเซ็นเซอร์ควัน
│   │
│   └── PLUB/                          # ระบบการประมวลผลเครือข่าย, ความร้อน และร้านค้าอุปกรณ์
│       ├── network/                   # แกนหลักของการจำลองเครือข่าย (Network Simulation Logic)
│       │   ├── NetworkManager.cs      # ระบบเบื้องหลังตรวจสอบเส้นทางว่าพอร์ตย่อยใดเชื่อมต่อกันบ้าง
│       │   ├── NetworkDevice.cs       # โครงสร้างตัวแทนของอุปกรณ์ Network แต่ละโหนด
│       │   └── NetworkPort.cs         # สถานะของพอร์ต (รับ-ส่ง Data)
│       │
│       ├── heat/                      # ระบบจำลองอุณหภูมิ (Thermodynamics)
│       │   ├── HeatManager.cs         # คุมอุณหภูมิรวม (Ambient Temperature) ระดับห้อง
│       │   └── DeviceHeat.cs          # คำนวณความร้อนพ่นออกจากอุปกรณ์แต่ละชิ้นตามการกินไฟ (Watt)
│       │
│       ├── device_info/               # ระบบจำลองซอฟต์แวร์ภายในอุปกรณ์ (OS Simulation)
│       │   ├── RouterInfoSetValue.cs  # คอนฟิก Routing Protocol และ IP สำหรับ Router
│       │   ├── FirewallInfoSetValue.cs# การตั้งค่า Rule ป้องกัน Network สำหรับคลาส Firewall
│       │   └── (ConfigIP/, routing_table/, lb_pool/) # โฟลเดอร์เก็บ Data Model ยูนิตย่อย
│       │
│       ├── store/                     # ระบบร้านค้าซื้ออุปกรณ์เพิ่มเติม
│       │   └── StoreController.cs     # ควบคุมการเลือกซื้อ Rack และ Server ผ่านปุ่มบน UI Store
│       │
│       ├── placement/                 # ระบบการวางผังอุปกรณ์ที่ซื้อมาลงในฉาก
│       │   ├── PlacementPreview.cs    # แสดง Ghost Model เช็คระยะชนระหว่างผนังกับสวิตช์ตัวใหม่
│       │   └── InputManager.cs        # ควบคุม Input จอยเวลาหมุน/ขยับโมเดลก่อนกดวาง
│       │
│       └── vr_keyboard_handler/       # ตัวรับข้อมูล Text ผ่านโฮโลแกรม
│           └── VRKeyboard.cs          # แป้นพิมพ์ VR สำหรับพิมพ์ Config IP และคำสั่งต่างๆ
├── PlayerScanner.cs                   # ตรวจจับพอร์ตเมื่อมือ VR หยิบสายเข้าใกล้เซิร์ฟเวอร์
└── ServerPlugScanner.cs               # รวมรายชื่อพอร์ตบน Device ให้เป็นศูนย์กลางรองรับส่วนต่อขยาย
```

**ตารางที่ 2.1 สรุปหน้าที่ของแต่ละไฟล์ในส่วนของระบบหลักต่างๆ (เรียงตามตัวโครงสร้างโปรเจกต์)**

| ที่ | ชื่อไฟล์ | หน้าที่ |
|---|---|---|
| 1. | Chomm/CableDataUI.cs | จัดการ UI แสดงข้อมูลรายละเอียดของสายเคเบิล |
| 2. | Chomm/DetectControllerPointing.cs | ตรวจจับและประมวลผลการชี้เป้าหมายของคอนโทรลเลอร์ VR |
| 3. | Chomm/EquipmentData.cs | จัดเก็บรูปแบบข้อมูลจำเพาะ (Specs) ของเบสอุปกรณ์เครือข่ายแต่ละชนิด |
| 4. | Chomm/GameSaveData.cs | โครงสร้างข้อมูล (Data Model) พื้นฐานใช้สำหรับสร้างชุด Save/Load |
| 5. | Chomm/SaveManager.cs | ฟังก์ชันหลักระบบบันทึกและโหลดสถานะจัดเก็บพิกัดของอุปกรณ์ทั้งหมด |
| 6. | Chomm/SceneLoader.cs | ควบคุมและจัดการกระบวนการโหลดสลับเปลี่ยน Scene ภายในเกมเพลย์ |
| 7. | Chomm/ScreenshotManager.cs | ระบบสคริปต์คำสั่งถ่ายและจัดบันทึกภาพหน้าจอภายในสภาพแวดล้อมจำลอง |
| 8. | Chomm/CableSystem/Cable.cs | ฟังก์ชันหลักคุมตรรกะทางโค้งงอฟิสิกส์ของตัวเส้นสายเคเบิล |
| 9. | Chomm/CableSystem/CablePlug.cs | รับรู้สถานะตรวจจับการเสียบเชื่อมต่อเข้าและดึงถอดออกของหัวปลั๊ก |
| 10. | Chomm/CableSystem/CableSpawner.cs | ระบบการจำลองและเสกจับสายเคเบิลจำลองสคริปต์ขึ้นมาในระบบ |
| 11. | Chomm/CableSystem/Port.cs | รักษาสถานะตัวแปรและตรรกะของฟิสิกส์ช่องพอร์ตที่ติดอยู่บนเครื่อง |
| 12. | Chomm/Environment/RoomGenerator.cs | ฟังก์ชันโปรแกรมหลักคุมขนาดกำแพงสภาพแวดล้อมจำลองแบบ Dynamic |
| 13. | Chomm/Environment/SlidingDoorController.cs | ควบคุมสถานะและแอนิเมชันเปิดปิดระบบประตูไฟฟ้าอัตโนมัติรอบห้อง |
| 14. | Chomm/Scenes/MainMenuScene.cs | ไฟล์การรวมฉากและพื้นที่ของสภาพแวดล้อมกล้อง Scene เข้าเมนู |
| 15. | Chomm/Scenes/PlayScene.cs | ข้อมูลฉากควบคุมตัวแปร Scene ประสบการณ์ห้องเซิร์ฟเวอร์หลักผู้เล่น |
| 16. | Chomm/Scenes/TestScene.cs | พื้นที่ฉากทดลองย่อย (Sandbox) สำหรับประมวลผลและทดสอบการออกแบบ |
| 17. | Chomm/ScriptableObjects/RoomTemplate.cs | โครงสร้างเทมเพลตสำเร็จรูปสำหรับกำหนดขนาดและตรรกะจัดวางห้องเริ่มต้น |
| 18. | Chomm/UI/MainMenuManager.cs | ดูแลระบบจัดการลอจิกคำสั่งปุ่มกดและการโต้ตอบในบริเวณ Main Menu UI |
| 19. | Chomm/UI/MyRoomManager.cs | ควบคุมระบบประมวลผล UI และการจัดการหน้าต่างของห้องบัญชีส่วนตัว |
| 20. | Chomm/UI/RoomCreatorManager.cs | ใช้งานประมวลผลหน้าจอตรรกะในขณะที่ผู้เล่นเซ็ตอัปเพื่อประกอบห้องเริ่มต้น |
| 21. | Scripts/DRAGON/CCTV/CCTVDetectionZone.cs | ประมวลผลพื้นที่เพื่อตรวจจับการเคลื่อนไหวของผู้เล่นขณะผ่านบริเวณกล้องวงจรปิด |
| 22. | Scripts/DRAGON/CCTV/CCTVPlayerMarket.cs | อ้างอิงเครื่องมือพิกัดตัวกระทำโต้ตอบเพื่ออนุญาตให้กล้องวงจรปิดล็อกเป้าผู้เล่นได้ |
| 23. | Scripts/DRAGON/CCTV/CCTVStatusRed.cs | สคริปต์ควบคุมสถานะเตือนภัยลามเข้าสู่การผลลัพธ์แสดงแสงไฟแดงของกล้อง |
| 24. | Scripts/DRAGON/Fire/DischargeNozzleFX.cs | จำลองระบบประมวลผลเอฟเฟกต์ควบคุมการพ่นสารดับเพลิงออกจากหัวฉีดฉุกเฉิน |
| 25. | Scripts/DRAGON/Fire/FireAlarmSystem.cs | รับรู้สัญญาณแกนกลางเตือนระบบรวบรวมเพลิงไหม้แจ้งเหตุภายในพื้นที่ |
| 26. | Scripts/DRAGON/Fire/FireResetButtonTrigger.cs | ควบคุมจำลองจุดกดปุ่มคำสั่ง Reset ยกเลิกสัญญาณเตือนไฟไหม้บนระนาบลอจิก |
| 27. | Scripts/DRAGON/Fire/FireSafetyAutoLayout.cs | จัดเตรียมชุดเครื่องมือตัวช่วยจัดตำแหน่งอุปกรณ์ความปลอดภัยให้ห้องอัตโนมัติ |
| 28. | Scripts/DRAGON/Fire/HornStrobeController.cs | ทำงานควบคู่วงจรเตือนภัยแฟลชแสงกระพริบและประมวลผลลำโพงไซเรน |
| 29. | Scripts/DRAGON/Fire/ManualCallPoint.cs | ระบบสคริปต์ควบคุมตรรกะสถานะทุบตู้กระจกปุ่มแดงแจ้งเหตุไฟไหม้ด้วยตนเอง |
| 30. | Scripts/DRAGON/Fire/MCPButtonTrigger.cs | รับรู้การตรวจจับโมเดลการเคลื่อนที่ของผู้เล่นที่กระแทกชนกระเด็นแจ้งระบบ |
| 31. | Scripts/DRAGON/Fire/RoomCenterMarker.cs | ค่าคงที่ของระบบเพื่อหาจุดการประมวลผลหาศูนย์กลางเพดานให้ครอบคลุมส่วนฉีดน้ำ |
| 32. | Scripts/DRAGON/Fire/SmokeDetector.cs | ตัวเก็บตรรกะและจัดการรับค่าปริมาณอนุภาคควันเพื่อการกระตุ้นจุดตรวจแจ้งเตือนเพลิง |
| 33. | Scripts/DRAGON/Fire/SmokeParticleTrigger.cs | ประมวลผลลอจิกคำนวณการชนและการพุ่งตกกระทบของ Particle กับโมเดลตรวจจับ |
| 34. | Scripts/DRAGON/Fire/SmokeRiser.cs | เป็นสคริปต์จัดการพฤติกรรมภาพเอฟเฟกต์ลักษณะการลอยของมวลควันปะทะบนเพดาน |
| 35. | Scripts/DRAGON/Fire/VRHUDCountdown.cs | ถ่ายทอดระบบค่าความเร็วตัวเลขนับถอยหลังพ่นสารควบคุมสู่อินเตอร์เฟซโฮโลแกรม |
| 36. | Scripts/DRAGON/Fire/VRHUDCountdownBinder.cs | แกนกลางคอยผูกข้อมูลเวลา UI เตือนให้ประสานเชื่อมกับหน้าจอเลนส์มุมมอง VR ผู้เล่น |
| 37. | Scripts/PLUB/device_info/RouteInfoSetValue.cs | จัดเก็บโครงสร้างระดับซอฟต์แวร์คอนฟิกแบบจำลองการกำหนด Protocol/IP ในเราเตอร์ |
| 38. | Scripts/PLUB/device_info/FirewallInfoSetValue.cs | จัดเก็บตรรกะคอนฟิกและการตั้งค่าสิทธิการจราจรกฎอนุญาต/ป้องกันเข้าใช้ของ Firewalls |
| 39. | Scripts/PLUB/heat/heatManager.cs | คอยเฝ้าระแวดระวังประมวลระบบการปรับลดตัวเลขอุณหภูมิ Ambient ระดับลอจิกพื้นที่รอบห้อง |
| 40. | Scripts/PLUB/heat/DeviceHeat.cs | แปลงผันค่าเป็นระบบคำนวณอัตราความร้อนเครื่องตามประจุกินพลังงานต่อพอร์ตไฟฟ้า (Watts) |
| 41. | Scripts/PLUB/network/NetworkManager.cs | ประมวลผลลอจิกคำนวณแบบจำลองเส้นทางตรรกะว่าการเสียบสายนำไปสู่การ Link อุปกรณ์ใด |
| 42. | Scripts/PLUB/network/NetworkDevice.cs | เปรียบเทียบเป็นเครื่องอ็อบเจ็กต์จำลองที่ถือสถานะและตาราง IP เครือข่ายตัวตนของโหนด |
| 43. | Scripts/PLUB/network/NetworkPort.cs | ลอจิกหน่วยความจำควบคุมสถิติรับ/ส่งแบนด์วิธและสถานะ On/Off เฉพาะย่อยบนฮาร์ดแวร์รูรับ |
| 44. | Scripts/PLUB/placement/PlacementPreview.cs | ใช้โมเดลพรีวิวภาพจำลองโปร่งแสงเพื่อการตัดสินใจและเช็คพิกัดก่อนผู้เล่นตกลงกดวาง |
| 45. | Scripts/PLUB/placement/InputManager.cs | แฮนเดิลจัดการทิศทางลอจิกคำสั่งปุ่มจากคอนโทรลเลอร์ซ้ายขวาในโหมดมุมมองหยิบจับซื้อของ |
| 46. | Scripts/PLUB/store/StoreController.cs | กลไกลอจิกพาเนลหน้าร้านค้าและประมวลปุ่มเพื่อ Instantiate ดึงแร็คโครงสร้างเข้าสู่สภาพจริง |
| 47. | Scripts/PLUB/vr_keyboard_handler/VRKeyboard.cs | ประมวลแผงข้อความโฮโลแกรมแป้นพิมพ์รับ Input จากการจำลองกด VR เพื่อตั้งคลาสโค้ดและ IP |

**หน้าที่หลักระดับเจาะจงของโฟลเดอร์/สคริปต์ที่ควรรู้:**
- **สคริปต์หน้าด่านใน `Assets/Chomm/`:** เป็นหัวใจหลัก เช่น `SaveManager.cs` มีหน้าที่บันทึกพิกัดของอุปกรณ์ทุกชิ้น และ `GameSaveData.cs` จะคอยเก็บ Format ของ JSON Data
- **`CableSystem` โฟลเดอร์:** ระบบโครงข่ายชิ้นส่วนเคเบิล ประกอบด้วย Class หลักคือ `Cable.cs` คุมเส้นสาย, `CablePlug.cs` คุมหัวปลั๊ก และ `Port.cs` คุมรูต่อ ซึ่งคุยกันผ่านระบบ Event
- **`Environment/RoomGenerator.cs`:** ดูแลการสร้างสภาพแวดล้อมห้อง (กำแพง, พื้น, รหัสห้อง) โดยใช้ร่วมกับโครงสร้างข้อมูลเพื่อ Generate แบบ Dynamic
- **`Scenes/MainMenuScene.unity`:** ควบคุมการเข้าออกตัวเกม ถูกคุมและจัดการด้วยสคริปต์ภายในโฟลเดอร์ `UI/` (เช่น `MainMenuManager.cs`) 
- สคริปต์ที่อยู่นอกสุด (`PlayerScanner.cs` / `ServerPlugScanner.cs`) เป็น Utility ที่ยึดติดกับ Player และ Object โดยตรงสำหรับการทำ Interaction เบื้องต้น

---

## 3) System Architecture Breakdown

### 3.1) Cable & Port Management System (ระบบจัดการสายสัญญาณ)
- **เป้าหมาย:** สร้างและจัดการ Logic สายเคเบิล/สายไฟ เชื่อมต่อ Port-to-Port รับรู้ได้ว่าสายเส้นไหนเสียบอยู่กับ Port อะไร
- **ไฟล์หลัก:** `PlayerScanner.cs`, `ServerPlugScanner.cs`, `CableDataUI.cs`
- **Component หลัก:** `SnapZone` (BNG), `CablePlug`, `Collider`
- **Data (In/Out):** รับเข้า `PlugType` (เช่น FiberLCMultimode, IECC13) ส่งออก Event ของการเชื่อมต่อ (Connected/Disconnected)
- **Lifecycle:** `Awake` (รวบรวม Ports ทั้งหมดใน Object) -> `OnTriggerEnter` (เช็คประเภทสาย) -> `HighlightPort` -> ผู้ใช้ปล่อยมือเสร็จสิ้น(Snap)

### 3.2) Player VR Interaction System (ระบบปฏิสัมพันธ์ผู้เล่น)
- **เป้าหมาย:** ให้ผู้เล่นใช้จอย VR หยิบ และเห็น Feedback ก่อนที่สายจะเสียบเข้าที่รูกริด 
- **ไฟล์หลัก:** `PlayerScanner.cs`, BNG Framework's `Grabber`
- **Component หลัก:** `AudioSource` (ให้เสียงคลิก/Snap), OnTriggerEnter
- **Data (In/Out):** รับสถานะว่า "มือถือกรรมสิทธิ์ใน Object ใดอยู่ (Hand Held Item)"
- **Dependencies:** ต้องพึ่งพา `BNG.Grabber` เพื่อห่วงสถานะการจับ

### 3.3) Save and State Management System (ระบบบันทึกสถานะ)
- **เป้าหมาย:** โหลดห้องเซิร์ฟเวอร์ อุปกรณ์ที่ติดแร็คอยู่ และข้อมูลสายสัญญาณขึ้นมาระหว่าง Scene หรือ Session 
- **ไฟล์หลัก:** `SaveManager.cs`, `GameSaveData.cs`
- **Dependency:** โยงกับ `ScriptableObjects` หรือ JSON Parsing

---

## 4) File-by-File Analysis

### `Assets/PlayerScanner.cs`
#### Purpose
ไฟล์นี้เป็น Utility ที่ทำงานคลุมมือผู้เล่น (VR Player) หน้าที่หลักคือ "สแกนหา Port ที่เข้ากันได้ (Compatible Ports) กับสายเคเบิลที่ผู้เล่นกำลังถืออยู่" และแสดงแถบสี หรือ Highlight เพื่อบอกใบ้ต่อผู้เล่น
#### Type
MonoBehaviour (Attached to Player's hand/scanner bounds)
#### Main Responsibilities
- ตรวจจับว่ามือผู้เล่นถือ `CablePlug` ประเภทไหนอยู่
- เช็ค Rule การเข้ากันได้ เช่น สาย Fiber ต้องเสียบพอร์ต Fiber (สนับสนุน Multimode / Singlemode overlap check)
- Highlight สีเขียว (อนุญาตให้เสียบ) หรือ สีแดง (ผิดชนิด) 
- ส่งเสียงเตือน Sound Effect (Sfx) เมื่อเข้าโซนและเสียบ
#### Key Classes / Structs / Enums
- ต้องอ้างอิง `Enum PlugType` (คาดว่าอยู่ใน `Chomm.CableSystem`)
#### Public Fields / Serialized Fields
- `TriggerSfx` (AudioClip): เสียงเมื่อมือเข้าไปในเซิร์ฟเวอร์โซน
- `PlugSfx` (AudioClip): (ยังไม่ถูกใช้งานชัดเจนใน Code ปัจจุบัน)
- `SnapSfx` (AudioClip): เสียงเมื่อ Snap สำเร็จ
#### Important Methods
- **`GetHeldPlug()`**
  - **Purpose:** หาและ Return `CablePlug` ที่ผู้เล่นดึงถืออยู่ในมือ
  - **Caller:** `OnTriggerEnter`, `OnTriggerStay` โดนเรียกเสมอตอนมือแช่ไว้ใน Collider
- **`HighlightPort(SnapZone port, bool canPlug)`**
  - **Purpose:** สั่งเปลี่ยนสีวงแหวน (SnapZoneRingHelper)
  - **Input:** ชิ้นส่วน port และ Boolean เช็คสิทธิ์การเข้ากันได้
  - **Output:** เปลี่ยน UI Shader color (เขียว/แดง)
#### Input / Output Summary
- **Input:** Trigger Box Collision
- **Output:** Color Highlight บนโมเดล, Audio Cues
#### Dependencies
- **[BNG] Grabber.cs:** เอาไว้ดู `HeldGrabbable`
- **ServerPlugScanner.cs:** เพื่อดึงตัวแปร `allPorts`
#### Risks / Notes
- Method `GetHeldPlug` ใช้ `FindObjectsOfType<Grabber>()` ในระดับ `OnTriggerEnter` ซึ่งอาจกิน Performance บ้าง หากทำ Caching ไว้ใน `Awake/Start` จะมีประสิทธิภาพกว่า 

---

### `Assets/ServerPlugScanner.cs`
#### Purpose
ไฟล์นี้จะอยู่บนโมเดล Server (หรือ Switch) ทำหน้าที่ "รวบรวมและถือ List พอร์ตเชื่อมต่อ (SnapZone) ทั้งหมดบนตัวเอง และอัปเดตแบบไดนามิกหากมีอุปกรณ์เสียบซ้อนเข้ามา"
#### Type
MonoBehaviour (Component on equipment)
#### Main Responsibilities
- ค้นหา `SnapZone` บน Hierarchy ของ Server ชิ้นนี้
- รองรับการ "เสียบอุปกรณ์เสริม" (เช่น โมดูล SFP) ที่พกพอร์ตของตัวเองติดมาด้วย (Nested Ports) โดยเพิ่มเข้าสู่ Pool `allPorts` 
#### Important Methods
- **`Awake()`**
  - **Side Effects:** ล้างและอัปเดต `allPorts` ด้วย `GetComponentsInChildren<SnapZone>()`
- **`RegisterFromSnapZone(SnapZone snapZone)`**
  - **Purpose:** เมื่อมีการเสียบไอเทมใหม่เข้ามาใน Server ระบบจะค้นหาว่า ไอเทมนี้มีพอร์ต (SnapZone) ติดมาด้วยไหม ถ้ามี ให้เอามาใส่รวมกับตะกร้า `allPorts` ของแม่ (Cascaded hierarchy)
  - **Called By:** ระบบ Event จาก BNG Framework (เมื่อ Item_snapped)
- **`GetMatchPorts(PlugType plugType)`**
  - **Return:** รายการพอร์ตทั้งหมดที่ Type ตรงกับ Parameter กรองเอาเฉพาะตัวที่เสียบได้ 
#### Input / Output Summary
- **Input:** Event การ Snap หรือ Unsnap อุปกรณ์
- **Output:** จำนวน Array List ของ `allPorts` ที่ขยายขนาดขึ้นหรือลดลงตามโมดูลโครงสร้าง
#### Dependencies
- `BNG.SnapZone` 
#### Risks / Notes
- ตรรกะ `RegisterFromSnapZone` ถือว่าฉลาดมากเพราะรองรับอุปกรณ์เสริม (Modular Hardware) แต่นักพัฒนาควรระวัง Infinite Loop ถ้า Hierarchy ไม่ชัดเจน 

---

### `Assets/Chomm/SaveManager.cs` (อิงจากการค้นพบและโครงสร้างชื่อไฟล์)
#### Purpose
ควบคุมการ Save และ Load โลกของห้องเซิร์ฟเวอร์ ให้สถานะยังคงอยู่เมื่อเปิดปิดแอป
#### Main Responsibilities
- แปลงโครงสร้างอุปกรณ์ แร็ค ตำแหน่งที่วาง (Transform Data) ให้เป็น Data Payload
- ซีเรียลไลซ์ (Serialize) เป็นฟอร์แมต JSON/Binary
- Instantiates อุปกรณ์กลับคืนมาตำแหน่งเดิมเมื่อ Load Game
#### Dependencies
- เชื่อมต่อกับคลาส `GameSaveData` เพื่อระบุ Schema
#### Flow Note
- เรียกเมื่อกดปุ่ม Save หรือออกจาก Scene คาดว่าจะจับคู่กับ UI 

---

### `Assets/Chomm/ScreenshotTrigger.cs` / `ScreenshotManager.cs`
#### Purpose
จัดการการถ่ายและจัดเก็บภาพสแนปชอตหน้าจอ (สำหรับการรายงานผลการเชื่อมต่อ หรือใช้ในแผงวงจรในเกม)
#### Main Responsibilities
- รับสัญญาณสั่งแคปหน้าจอจากปุ่ม (UI หรือจอย)
- บันทึก Texture2D สู่ไฟล์ (PNG/JPG) ไปยัง Path ที่ซ่อนไว้ หรือแสดงบน UI แบบไดนามิก

---

## 5) Cross-File Relationship Map

โครงข่ายความสัมพันธ์ระหว่างไฟล์สามารถเขียนแบบ Trace Flow เพื่อนำไปอธิบายคู่มือพัฒนาได้ดังนี้:

**Flow: การหยิบและเสียบสายเคเบิล (Main Interaction Loop)**
1. **หยิบสายไฟ:** ผู้ใช้งาน (Player) กดจอยคอนโทรลเลอร์ -> `BNG Framework` จะเชื่อมผูก GameObject ของสายไฟ (ซึ่งถือ `CablePlug`) เข้ากับ `Grabber` บนมือผู้เล่น 
2. **ขยับเข้าใกล้แร็ค:** มือผู้เล่นซึ่งติด `PlayerScanner.cs` จะ Trigger เข้าชนกับ Server ที่มีแท็ก `ServerPlugScanner.cs`
3. **ตรวจสอบพอร์ต:** `PlayerScanner.cs` เรียกฟังก์ชันเช็คพอร์ตไปยัง Server เพื่อขอ `allPorts`
4. **แสดงไฮไลท์:** `PlayerScanner.cs` เทียบ Enum `PlugType` ถ้าของในมือตรงกับพอร์ต ระบบจะทำ `Highlight(Color.green)` บนโมเดล Port นั้นๆ
5. **ทำการเสียบ (Snap):** เมื่อผู้ใช้ปล่อยปุ่มหยิบใกล้ๆ โซน `SnapZone` จะดูดหัวเคเบிலเข้าที่
6. **อัปเดตสเตท:** Event OnSnap จะถูกส่งไปยังโมดูลย่อย (`CableSystem`) เพื่อระบุค่าว่า Router ชุดนี้ได้รับการเชื่อมต่อกับโหนดปลายทางแล้ว

**Flow: การเสียบอุปกรณ์เสริม (Modular Addition เช่นเสียบ SFP Transceiver เข้า Switch)**
1. เสียบ SFP เข้า Switch -> `SnapZone` ยิง callback แจ้งเตือน
2. แจ้งไปยัง `ServerPlugScanner.cs` ให้ทำงาน Method: `RegisterFromSnapZone` 
3. Script นี้จะตรวจค้นในชิ้นงานที่เพิ่งเสียบว่า "มีกี่พอร์ตซ่อนอยู่" และบวกพอร์ตเหล่านั้นเข้าวงจรของ Switch ตัวหลัก 

---

## 6) Scene and Prefab Mapping

**Scene หลัก**
- **TestScene.unity / TestScene Backup.unity:** สำหรับใช้ Development ทดสอบ Environment แยกชิ้น
- **PlayScene.unity:** Scene หลักสำหรับโหมดการเล่น/โหมดใช้งานจริง (Production Entry)
- **plub example.unity:** Scene ทดสอบโมดูลของคุณ Plub/ระบบความร้อน-Network

**Prefabs สำคัญ**
โครงสร้างโปรเจกต์นี้มี Prefab ลึกซึ้งเกี่ยวกับการจำแนกสายเคเบิล:
- `Cable V1 Rj45.prefab` / `Cable V1 FiberLCSinglemode.prefab` / `Cable V1 FiberLCMultimode.prefab` / `Cable V1 Power.prefab` 
- **ความสำคัญ:** สายทุกเส้นถูกทำเป็น Prefab แยกเพื่อระบุสีและลักษณะโครงสร้างทางฟิสิกส์ (`PlugType`) นักพัฒนาห้ามสับเปลี่ยน Script ของ PlugType โดยเด็ดขาด
- `rack42U_nodoor Prefab.prefab` / `c9200.prefab` (Cisco Switch)
- **อุปกรณ์รับเครือข่าย:** จะมี GameObject `Anchor Port` และ `SnapZone` ซ้อนอยู่ภายใน ต้องถูกเซ็ตอย่างถูกต้องใน Inspector หน้าที่ของนักพัฒนาใหม่คือหากทำ 3D Model Switch ตัวใหม่ ต้องวาง SnapZone ตามจุดรูพอร์ตให้แม่นยำ

---

## 7) Data Flow and State Flow

* อ้างอิงสำหรับเอาไปเขียน Developer Manual ในหมวดกระแสข้อมูล *

1. **Hardware State Flow:** 
   - ตัวแปรตั้งต้นคือ ข้อมูลอุปกรณ์อยู่ใน `EquipmentData` -> ผู้ใช้วางแร็ค -> Object Instantiated 
   - State คือ "Off" (ไม่มีไฟ)
   - เมื่อ User เสียบ `Cable V1 Power` เข้าสู่ Port พาวเวอร์ซัพพลาย -> ส่ง Event ไปยัง `device_info` หรือ `heat` Manager เปลี่ยน State เป็น "Running/Powered"
2. **Network Data Flow:**
   - สาย RJ45 เชื่อมต่อ Interface A กับ Interface B
   - `CableSystem` ตรวจสอบความถูกต้อง ส่งข้อมูลต่อไปยัง `NetworkManager` 
   - อัปเดตตารางเส้นทาง (Routing Table / Pathfinding graph) แบบเบื้องหลัง เพื่อแสดงว่า Server เครื่อง 1 ปิง (Ping) เจอ เครื่อง 2 แล้ว

---

## 8) Subroutine / Function Documentation for Manual Use

ตารางสรุป Method สำหรับเอกสาร Developer Manual ระดับ Technical:

| File | Function / Method | Purpose | Input Parameters | Output / Return | Called By | Affects State |
|---|---|---|---|---|---|---|
| PlayerScanner.cs | OnTriggerEnter(Collider) | จุดเริ่มต้นเมื่อผู้เล่นชี้ไปที่ Server เพื่่อค้นหาพอร์ต | Collider other | - | Unity Engine (Physics) | เสียงแจ้งเตือน, State UI |
| PlayerScanner.cs | GetHeldPlug() | ค้นหาว่า Player ถือปลั๊กประเภทอะไรอยู่เพื่อนำไป Compare | - | CablePlug object | OnTriggerEnter/Stay | - |
| PlayerScanner.cs | HighlightPort() | อัปเดตสีล้อมรอบวงแหวนพอร์ตเพื่อช่วยเหลือนักเรียนผู้ใช้งาน | SnapZone port, bool canPlug | - | - | เปลี่ยนสีวงแหวนเป็นเขียว/แดง |
| ServerPlugScanner.cs | RegisterFromSnapZone() | รวมพอร์ตย่อยจากไอเทมสวมใส่เสริมเข้าไปในพอร์ตทั้งหมดของเซิร์ฟเวอร์แม่ | SnapZone snapZone | - | BNG Snap Event Callback | เพิ่มจำนวนใน allPorts List |
| ServerPlugScanner.cs | UnregisterFromSnapZone() | ถอดทิ้งพอร์ตย่อยเมื่อผู้ใช้ดึงโมดูลเสริมออกจากเร้าเตอร์ | SnapZone snapZone | - | BNG Unsnap Callback | ลบ Object ใน allPorts List |

---

## 9) Candidate Diagrams to Draw in Developer Manual

*ผมขอแนะนำให้นำ List นี้ไปวาด Flowchart ภาพลงในปริญญานิพนธ์ได้เลยครับ*

1. **System Architecture Diagram (ภาพรวม):**
   - **Nodes:** VR Hand Layer, Cable Engine (Chomm), Simulation Engine (Plub), Save/Load DB
   - **Arrows:** หยิบจับ (Input) -> ตรวจสอบชนิดปลั๊ก (Logic) -> อัปเดตพารามิเตอร์เซิร์ฟเวอร์ -> อัปเดตหน้าจอโฮโลแกรม
2. **Key Interaction Flow Diagram (การเชื่อมต่อเคเบิลเสมือนจริง):**
   - **Nodes (Flowchart):** User Picks Cable -> OnTriggerEnter -> `Is it holding a plug?` -> [Yes] Get PlugType -> `Check compatibility with Ports` -> [Match] Green Ring / [Fail] Red Ring -> User Let Go -> Snap Triggered.
3. **Modular Port Registration Flow Diagram:**
   - **Scenario:** เอาไว้ใช้อธิบายความก้าวหน้าของโปรเจกต์ว่า "เรารองรับ SFP Module"
   - **Nodes:** User Inserts Unit -> Event: OnSnap -> `RegisterFromSnapZone` -> Iterator (get child snaps) -> `allPorts.Add()`.

---

## 10) Suggested Developer Manual Outline

แนะนำโครงสร้างสารบัญสำหรับไปจัดพิมพ์ลงไฟล์ `.docx`:

**บทที่ 1: บทนำและภาพรวมของระบบเครือข่ายจำลอง**
  1.1 สถาปัตยกรรมระดับซอฟต์แวร์ (Unity & C# Scripting)
  1.2 การแบ่งส่วนรับผิดชอบ (Module Separation: Chomm, PLUB, DRAGON)
**บทที่ 2: โครงสร้างไฟล์และการจัดเก็บ Asset**
  2.1 โครงสร้างเชิงลึก (Hierarchy Directory)
  2.2 โมเดลและ Prefab มาตรฐาน (Cable, Server Rack, Modules)
**บทที่ 3: ระบบการโต้ตอบด้วยเสมือนจริง (VR Interaction Mechanics)**
  3.1 การเชื่อมโยง BNG Framework สู่สคริปต์
  3.2 ตรรกะของ `PlayerScanner` และการอ่านค่าถือครอง
**บทที่ 4: กลไกสายสัญญาณและสถานะเซิร์ฟเวอร์ (Cable & Port Logic)**
  4.1 การเข้ากันได้ของพอร์ต (Port Compatibility Logic)
  4.2 ระบบรวบรวมพอร์ตอัตโนมัติ (`ServerPlugScanner`) 
**บทที่ 5: ระบบเครือข่ายและความร้อน (Network & Environment Simulation)**
  5.1 กระแสข้อมูลการทำงานเบื้องหลัง 
**บทที่ 6: การบันทึกและระบบฐานข้อมูล (File Saving & Data Pipeline)**
  6.1 กระบวนการนำเข้าและส่งออกข้อมูล Layout เครือข่าย  
**บทที่ 7: คู่มือการเพิ่มอุปกรณ์ใหม่ (How to Add New Equipments)**
  7.1 ขั้นตอนสร้าง Prefab อุปกรณ์ใหม่ 
  7.2 กฎการวาง SnapZone และการตั้งค่า Layer
**ภาคผนวก:**
  - สรุป Function / API Reference
  - Flowcharts (ที่วาดจาก Section 9)

---

### [Missing Information / Need Manual Verification]
_จุดที่นักพัฒนาควรเช็คใน Unity Editor ด้วยตัวเองเพิ่มเติมเพื่อให้คู่มือนี้เป๊ะที่สุด:_
1. **Event Callbacks in Inspector:** การเรียก `RegisterFromSnapZone` บน `ServerPlugScanner` ถูกลากเส้นผ่าน Unity Events บน `SnapZone` component ใน Inspector หรือไม่ กรุณาเปิด Prefab `c9200.prefab` แล้วเช็คที่กรอบ UnityEvent `OnSnap` เพื่อเพิ่มเป็นรูปภาพประกอบคู่มือ
2. **Save Data JSON Schema:** จำเป็นต้องเปิดดูไฟล์ที่เซฟออกมาจากการรันจริง (เช่นใน AppData) ว่าหน้าตาสครงสร้างของ `GameSaveData` มี Property ครบถ้วนตามเอกสารหรือไม่
3. **Enum `PlugType`:** ตรวจสอบชื่อ Enum ทั้งหมด (Power, Rj45, FiberLCSinglemode, FiberLCMultimode, Dsl) เพื่อตีกรอบข้อมูลลง Document ให้ครบถ้วน
