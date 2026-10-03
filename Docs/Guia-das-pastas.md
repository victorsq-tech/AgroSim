# Onde ficam as coisas no AgroSim

A raiz do jogo é a pasta AgroSim, dentro deste projeto local. Abra essa pasta pelo Unity Hub.

| Pasta | Conteúdo |
|---|---|
| Assets/AgroSim/Scenes | Cena principal Gameplay.unity |
| Assets/AgroSim/Scripts/Core | Estado do jogo, seleção de equipamentos e manobra do galpão |
| Assets/AgroSim/Scripts/Farm | Dados da fazenda |
| Assets/AgroSim/Scripts/Tractors | Dados dos tratores |
| Assets/AgroSim/Scripts/Operations | Operações, etapas e aparência do terreno trabalhado |
| Assets/AgroSim/Scripts/UI | Menus, galpão, painel inferior, estrada e ambiente visual |
| Assets/AgroSim/Scripts/Data | Save, economia e registros locais |
| Assets/AgroSim/Scripts/Events | Pasta preparada para eventos próprios |
| Assets/AgroSim/Scripts/Communication | Pasta preparada para comunicação; MQTT ainda não implementado |
| Assets/AgroSim/Prefabs | Modelos reutilizáveis de tratores, implementos e interface |
| Assets/AgroSim/Sprites | Imagens próprias do projeto |
| Assets/AgroSim/Resources | Configuração CozyFarmEnvironment.asset, usada para carregar grama, terra, piso, árvores e casas |
| Assets/AgroSim/Editor | Ferramentas de configuração e verificações executadas no Unity Editor |
| Assets/AgroSim/Data | Pastas previstas para dados de eventos, tratores e operações |
| Assets/AgroSim/Animations e Audio | Pastas preparadas para animações e sons |
| Assets/Skyden_Games/Cozy Farm | Pacote original importado pelo usuário; não mover seus arquivos individualmente |
| Docs | Registro das etapas, decisões, assets utilizados e este guia |

## Ambiente Cozy Farm

- Grama original: Assets/Skyden_Games/Cozy Farm/Textures/Ground Tiles.png, sprite Ground Tiles_0.
- Árvores originais: Assets/Skyden_Games/Cozy Farm/Prefabs/Props/Tree_0.prefab até Tree_3.prefab.
- Casas originais: Assets/Skyden_Games/Cozy Farm/Prefabs/Props/House_0.prefab e House_1.prefab.
- Ligação com o jogo: Assets/AgroSim/Resources/CozyFarmEnvironment.asset (grama, terra, piso, árvores e casas).
- Cores e botões compartilhados: Assets/AgroSim/Scripts/UI/FarmTheme.cs.
- Exibição e descarte das cópias temporárias das texturas: Assets/AgroSim/Scripts/UI/FarmArtwork.cs.
- Montagem e posicionamento durante o jogo: Assets/AgroSim/Scripts/UI/FarmTravelView.cs.
- O galpão é desenhado por GarageLayout.cs, e os menus por FarmMenus.cs, na mesma pasta UI.

As telas e algumas decorações são criadas ao entrar em Play. Por isso aparecem na Hierarchy durante a execução, e não como objetos permanentemente salvos na cena. O pacote do fornecedor fica separado das alterações próprias, que estão em Assets/AgroSim.

## Progresso salvo no PC

O save e os registros das operações ficam fora de Assets, em:
C:/Users/rotci/AppData/LocalLow/DefaultCompany/AgroSim/AgroSim

Library, Temp e Logs são pastas de trabalho geradas pelo Unity; o conteúdo principal do projeto é Assets, Packages, ProjectSettings e Docs. A estrutura atual foi mantida para preservar as referências existentes.

## Conferências sem alterar seu progresso

Assets/AgroSim/Editor/IsolatedPlaytest.cs acrescenta dois comandos no menu AgroSim > Testes do Unity: iniciar uma sessão com save isolado e retomar o último save de teste. Save e telemetria dessas sessões ficam em uma pasta temporária própria, com aviso no topo do jogo. Ao sair de Play, a seleção de teste é desativada; apertar Play normalmente usa seu progresso original.
