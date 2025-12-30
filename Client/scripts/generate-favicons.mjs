import favicons from "favicons";
import { writeFile, mkdir } from "fs/promises";
import path from "path";

const source = "src/assets/images/favicon.svg";

const configuration = {
  path: "/",
  appName: "Polluxkart",
  appShortName: "Polluxkart",
  icons: {
    android: true,
    appleIcon: true,
    favicons: true,
  },
};

const outputDir = "public";

const response = await favicons(source, configuration);

// write images
await mkdir(outputDir, { recursive: true });
for (const image of response.images) {
  await writeFile(path.join(outputDir, image.name), image.contents);
}

// write HTML snippet
await writeFile(path.join(outputDir, "favicons-snippet.html"), response.html.join("\n"));
