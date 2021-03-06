#addin "Cake.DotNetCoreEf&version=0.8.0"

var target = Argument("target", "Build");
var publishPath = Argument("publishPath", @"C:\Development\CHC\deploy\");
var runtime = "win-x64";

var targetDir = DateTime.Now.ToString("yyyyMMdd.HHmm");

string PublishPath(string extension) => System.IO.Path.Combine(publishPath, targetDir, extension);

void ScriptMigrations(string context, string filename) {
    DotNetCoreEfMigrationScript(
            @".\CHC.Consent.Api",
            new DotNetCoreEfMigrationScriptSettings {
                Context = context,
                Output = PublishPath(filename),
                Idempotent = true,
                //NoBuild = true
            }
        );
}

Task("Restore")
    .Does(() => {
        DotNetCoreRestore(
            new DotNetCoreRestoreSettings {
                Runtime = runtime
            }
        );
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
        DotNetCoreBuild(
            ".",
            new DotNetCoreBuildSettings{
                NoRestore = true,
                Runtime = runtime
            }
        );
    });

Task("PublishWeb")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCorePublish(
            @".\CHC.Consent.Api",
            new DotNetCorePublishSettings {
                Runtime = runtime,
                OutputDirectory = PublishPath("web"),
                NoBuild = true,
            }
        );
    });

Task("PublishDataTool")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCorePublish(
            @".\CHC.Consent.DataTool",
            new DotNetCorePublishSettings {
                Runtime = runtime,
                OutputDirectory = PublishPath("datatool"),
                NoBuild = true,
                SelfContained = true
            }
        );
    });

Task("Script-Consent")
    .Does(() => {
        ScriptMigrations("ConsentContext", "001.sql");
    });

Task("Script-Configuration")
    .Does(() => {
        ScriptMigrations("ConfigurationDbContext", "002.sql");
    });

Task("Script-PersistedGrant")
    .Does(() => {
        ScriptMigrations("PersistedGrantDbContext", "003.sql");
    });

Task("Script-Migrations")    
    .IsDependentOn("Build")
    .IsDependentOn("Script-Consent")
    .IsDependentOn("Script-Configuration")
    .IsDependentOn("Script-PersistedGrant")
    .Does(() => {});
    

Task("Publish")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("PublishDataTool")
    .IsDependentOn("Script-Migrations")
    .Does(() => {
        Zip(PublishPath(""), System.IO.Path.Combine(publishPath, $"{targetDir}.zip"));
    });

RunTarget(target);