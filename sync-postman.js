const fs = require('fs');
const path = require('path');

async function syncToPostman() {
  let apiKey = process.env.POSTMAN_API_KEY || process.argv[2];
  const workspaceId = process.env.POSTMAN_WORKSPACE_ID || process.argv[3] || '638d85d2-e047-4731-8287-3957d01bd713';

  if (!apiKey) {
    try {
      const mcpConfigPath = path.join(__dirname, '.agents', 'mcp_config.json');
      if (fs.existsSync(mcpConfigPath)) {
        const mcpConfig = JSON.parse(fs.readFileSync(mcpConfigPath, 'utf8'));
        apiKey = mcpConfig?.mcpServers?.postman?.env?.POSTMAN_API_KEY;
      }
    } catch {}
  }

  if (!apiKey || apiKey === 'YOUR_POSTMAN_API_KEY_HERE') {
    console.error('Error: A valid Postman API Key is required.');
    console.error('Provide it in .agents/mcp_config.json, via environment variable POSTMAN_API_KEY, or as an argument:');
    console.error('   node sync-postman.js <YOUR_POSTMAN_API_KEY>');
    process.exit(1);
  }

  const collectionFilePath = path.join(__dirname, 'Survey_System_API.postman_collection.json');
  if (!fs.existsSync(collectionFilePath)) {
    console.error(`Error: Collection file not found at ${collectionFilePath}`);
    process.exit(1);
  }

  const collectionData = JSON.parse(fs.readFileSync(collectionFilePath, 'utf8'));
  const collectionName = collectionData.info.name;

  console.log(`Connecting to Postman API with API Key: ${apiKey.substring(0, 10)}...`);

  try {
    // 1. Check existing collections in the workspace/account
    const listRes = await fetch('https://api.getpostman.com/collections', {
      headers: { 'X-Api-Key': apiKey }
    });

    if (!listRes.ok) {
      const errorData = await listRes.json();
      console.error(`Postman API Error (${listRes.status}):`, JSON.stringify(errorData, null, 2));
      process.exit(1);
    }

    const { collections } = await listRes.json();
    const existing = collections.filter(c => c.name === collectionName);

    let targetUid = null;

    if (existing.length > 0) {
      // Sort existing by newest first
      existing.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
      targetUid = existing[0].uid;
      console.log(`Found existing collection "${collectionName}" (UID: ${targetUid}).`);

      // Clean up older duplicate collections if any
      if (existing.length > 1) {
        console.log(`Cleaning up ${existing.length - 1} older duplicate collection(s)...`);
        for (let i = 1; i < existing.length; i++) {
          const dup = existing[i];
          try {
            await fetch(`https://api.getpostman.com/collections/${dup.uid}`, {
              method: 'DELETE',
              headers: { 'X-Api-Key': apiKey }
            });
            console.log(`- Removed stale duplicate: ${dup.uid} (created ${dup.createdAt})`);
          } catch (e) {
            console.warn(`- Failed to remove duplicate ${dup.uid}:`, e.message);
          }
        }
      }
    }

    if (targetUid) {
      // 2. In-place update existing collection
      console.log(`Updating collection "${collectionName}" (UID: ${targetUid}) in-place...`);
      const updateRes = await fetch(`https://api.getpostman.com/collections/${targetUid}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'X-Api-Key': apiKey
        },
        body: JSON.stringify({ collection: collectionData })
      });

      const updateData = await updateRes.json();
      if (!updateRes.ok) {
        console.error(`Postman API Update Error (${updateRes.status}):`, JSON.stringify(updateData, null, 2));
        process.exit(1);
      }

      console.log(`Collection successfully updated in Postman!`);
      console.log(`Collection Name: ${updateData.collection.name}`);
      console.log(`Collection ID:   ${updateData.collection.id}`);
      console.log(`Collection UID:  ${updateData.collection.uid}`);
    } else {
      // 3. Create new collection
      console.log(`Creating new collection "${collectionName}" in Postman...`);
      let createUrl = 'https://api.getpostman.com/collections';
      if (workspaceId) {
        createUrl += `?workspace=${encodeURIComponent(workspaceId)}`;
      }

      const createRes = await fetch(createUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Api-Key': apiKey
        },
        body: JSON.stringify({ collection: collectionData })
      });

      const createData = await createRes.json();
      if (!createRes.ok) {
        console.error(`Postman API Create Error (${createRes.status}):`, JSON.stringify(createData, null, 2));
        process.exit(1);
      }

      console.log(`Collection successfully created in Postman!`);
      console.log(`Collection Name: ${createData.collection.name}`);
      console.log(`Collection ID:   ${createData.collection.id}`);
      console.log(`Collection UID:  ${createData.collection.uid}`);
    }
  } catch (err) {
    console.error('Failed to communicate with Postman API:', err.message);
    process.exit(1);
  }
}

syncToPostman();
