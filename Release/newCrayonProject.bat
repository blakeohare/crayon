@echo OFF
ECHO New Crayon Project Creator
ECHO:
set /p cryProjId="Project ID (alphanumeric, must not begin with a number): "
set /p cryProjType="Project type (basic/game/ui, default basic): "
set /p cryProjDir="Target directory (default current directory): "
crayon.exe -ext:DefaultProject.type=%cryProjType% -ext:DefaultProject.projectId=%cryProjId% -ext:DefaultProject.targetDir=%cryProjDir%
