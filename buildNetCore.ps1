properties {
    $root =                          $psake.build_script_dir
    $configuration =                 "Release"
    $build_dir =                     "$root\build\"
    $build_dir_net_core =            "$build_dir\NetCore"
    $nunit_dir =                     "$root\packages\NUnit.Runners.2.6.2\tools\"
    $tools_dir =                     "$root\tools"
    $nuget_dir =                     "$root\.nuget"
    $env:Path +=                     ";$nunit_dir;$tools_dir;$nuget_dir"
}

task default -depends Compile

task Clean {
    remove-item -force -recurse $build_dir_net_core -ErrorAction SilentlyContinue | Out-Null
}

task Init -depends Clean {
    new-item $build_dir_net_core -itemType directory | Out-Null
}

task Compile -depends Init {
    exec { dotnet pack .\Mindscape.Raygun4Net.NetCore\ --output build\NetCore --configuration Release }
    move-item -Path $root\Mindscape.Raygun4Net.NetCore\build\NetCore\* -Destination $build_dir_net_core
}